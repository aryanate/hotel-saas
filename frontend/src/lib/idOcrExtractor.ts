import Tesseract from "tesseract.js"
import * as pdfjsLib from "pdfjs-dist"
import pdfWorkerUrl from "pdfjs-dist/build/pdf.worker.min.mjs?url"

pdfjsLib.GlobalWorkerOptions.workerSrc = pdfWorkerUrl

export type SupportedIdType =
  | "MaskedAadhaar"
  | "Passport"
  | "DrivingLicense"
  | "VoterID"
  | "PAN"

export interface ExtractedIdDetails {
  idType?: SupportedIdType
  idNumber?: string
  fullName?: string
  address?: string
  mobile?: string
  extractedFields: string[]
  rawText: string
}

export class PdfPasswordRequiredError extends Error {
  fileName: string
  incorrectPassword: boolean

  constructor(fileName: string, incorrectPassword = false) {
    super(`Password required for PDF: ${fileName}`)
    this.name = "PdfPasswordRequiredError"
    this.fileName = fileName
    this.incorrectPassword = incorrectPassword
  }
}

const NOISE_PHRASES = [
  "government of india",
  "govt of india",
  "unique identification",
  "authority of india",
  "uidai",
  "mera aadhaar",
  "meri pehchan",
  "aadhaar",
  "income tax",
  "permanent account",
  "election commission",
  "driving licence",
  "driving license",
  "republic of india",
  "passport",
  "enrolment",
  "enrollment",
  "download",
  "issue date",
  "valid till",
  "signature",
  "father",
  "husband",
  "mother",
  "address",
  "female",
  "male",
  "transgender",
  "year of birth",
  "dob",
  "yob",
  "vid",
  "help@",
  "www.",
]

function isBoilerplateLine(line: string): boolean {
  const lower = line.toLowerCase()
  return NOISE_PHRASES.some((phrase) => lower.includes(phrase))
}

function cleanAsciiLine(line: string): string {
  return line
    .replace(/[^\x20-\x7E]/g, " ")
    .replace(/\s+/g, " ")
    .trim()
}

function cleanNameCandidate(line: string): string {
  return line
    .replace(/[^A-Za-z.\s]/g, " ")
    .replace(/\s+/g, " ")
    .trim()
}

function isLikelyPersonName(line: string): boolean {
  if (!line || isBoilerplateLine(line)) return false
  if (/\d/.test(line)) return false
  const cleaned = cleanNameCandidate(line)
  const words = cleaned.split(" ").filter((w) => w.length >= 2)
  if (words.length < 2 || words.length > 4) return false
  if (cleaned.length < 5 || cleaned.length > 45) return false
  const alphaRatio = cleaned.replace(/\s/g, "").length / Math.max(1, line.trim().length)
  return alphaRatio > 0.75
}

function toTitleCase(name: string): string {
  return name
    .toLowerCase()
    .split(/\s+/)
    .filter(Boolean)
    .map((w) => w.charAt(0).toUpperCase() + w.slice(1))
    .join(" ")
}

async function preprocessImageForOcr(file: File): Promise<string> {
  const objectUrl = URL.createObjectURL(file)
  try {
    const img = await new Promise<HTMLImageElement>((resolve, reject) => {
      const el = new Image()
      el.onload = () => resolve(el)
      el.onerror = () => reject(new Error("Could not load image"))
      el.src = objectUrl
    })

    const maxDim = 1400
    let width = img.naturalWidth || img.width
    let height = img.naturalHeight || img.height
    if (width > maxDim || height > maxDim) {
      const ratio = Math.min(maxDim / width, maxDim / height)
      width = Math.round(width * ratio)
      height = Math.round(height * ratio)
    }

    const canvas = document.createElement("canvas")
    canvas.width = width
    canvas.height = height
    const ctx = canvas.getContext("2d")
    if (!ctx) return objectUrl

    ctx.filter = "grayscale(100%) contrast(135%) brightness(105%)"
    ctx.drawImage(img, 0, 0, width, height)
    return canvas.toDataURL("image/png")
  } finally {
    URL.revokeObjectURL(objectUrl)
  }
}

async function extractTextFromPdf(
  file: File,
  password?: string,
  onProgress?: (percent: number) => void
): Promise<string> {
  const arrayBuffer = await file.arrayBuffer()
  const loadingTask = pdfjsLib.getDocument({
    data: new Uint8Array(arrayBuffer),
    password: password || undefined,
  })

  let pdf: pdfjsLib.PDFDocumentProxy
  try {
    pdf = await loadingTask.promise
  } catch (err: unknown) {
    const errObj = err as { name?: string; code?: number }
    if (errObj?.name === "PasswordException") {
      throw new PdfPasswordRequiredError(file.name, errObj.code === 2)
    }
    throw err
  }

  const maxPages = Math.min(pdf.numPages, 3)
  const textParts: string[] = []

  for (let p = 1; p <= maxPages; p++) {
    const page = await pdf.getPage(p)
    const content = await page.getTextContent()
    const strings = content.items
      .map((item) => ("str" in item ? item.str : ""))
      .filter(Boolean)
    if (strings.length > 0) {
      textParts.push(strings.join("\n"))
    }
    if (onProgress) onProgress(Math.round((p / maxPages) * 40))
  }

  const embeddedText = textParts.join("\n").trim()
  // If digital PDF has meaningful selectable English text, return immediately!
  if (embeddedText.replace(/[^A-Za-z0-9]/g, "").length >= 40) {
    if (onProgress) onProgress(100)
    return embeddedText
  }

  // Otherwise scanned PDF -> render pages to canvas and run Tesseract OCR
  const ocrParts: string[] = []
  const ocrPages = Math.min(pdf.numPages, 2)
  for (let p = 1; p <= ocrPages; p++) {
    const page = await pdf.getPage(p)
    const viewport = page.getViewport({ scale: 2.0 })
    const canvas = document.createElement("canvas")
    canvas.width = Math.round(viewport.width)
    canvas.height = Math.round(viewport.height)
    const ctx = canvas.getContext("2d")
    if (!ctx) continue

    await page.render({ canvasContext: ctx, viewport, canvas }).promise
    const dataUrl = canvas.toDataURL("image/png")
    const res = await Tesseract.recognize(dataUrl, "eng", {
      logger: (m) => {
        if (m.status === "recognizing text" && typeof m.progress === "number" && onProgress) {
          const base = 40 + Math.round(((p - 1 + m.progress) / ocrPages) * 60)
          onProgress(base)
        }
      },
    })
    if (res.data.text) ocrParts.push(res.data.text)
  }

  return ocrParts.join("\n")
}

export function parseIndianIdText(rawText: string): ExtractedIdDetails {
  const lines = rawText
    .split(/\r?\n/)
    .map((l) => cleanAsciiLine(l))
    .filter((l) => l.length > 1)

  const upperText = rawText.toUpperCase()
  let idType: SupportedIdType | undefined
  let idNumber: string | undefined
  let fullName: string | undefined
  let address: string | undefined
  let mobile: string | undefined

  // 1. Passport MRZ check
  const mrzMatch = upperText.match(/P<IND([A-Z]+)<<([A-Z<]+)/)
  if (mrzMatch || upperText.includes("PASSPORT") || upperText.includes("REPUBLIC OF INDIA")) {
    idType = "Passport"
    if (mrzMatch) {
      const surname = mrzMatch[1].replace(/</g, " ").trim()
      const given = mrzMatch[2].replace(/</g, " ").trim()
      const combined = `${given} ${surname}`.trim()
      if (combined.length >= 3) fullName = toTitleCase(combined)
    }
    const passNo = upperText.match(/\b([A-Z][0-9]{7})\b/)
    if (passNo) idNumber = passNo[1]
  }

  // 2. PAN Card check
  const panMatch = upperText.match(/\b([A-Z]{5}[0-9]{4}[A-Z])\b/)
  if (!idType && (panMatch || upperText.includes("INCOME TAX") || upperText.includes("PERMANENT ACCOUNT"))) {
    idType = "PAN"
    if (panMatch) idNumber = panMatch[1]
  }

  // 3. Driving License check
  const dlMatch = upperText.match(/\b([A-Z]{2}[-\s]?\d{2}[-\s]?(?:19|20)\d{2}[-\s]?\d{6,7})\b/)
  if (!idType && (dlMatch || upperText.includes("DRIVING LICEN"))) {
    idType = "DrivingLicense"
    if (dlMatch) idNumber = dlMatch[1].replace(/\s+/g, "-")
  }

  // 4. Voter ID check
  const voterMatch = upperText.match(/\b([A-Z]{3}[0-9]{7})\b/)
  if (!idType && (upperText.includes("ELECTION COMMISSION") || (voterMatch && !panMatch))) {
    idType = "VoterID"
    if (voterMatch) idNumber = voterMatch[1]
  }

  // 5. Aadhaar Card check
  const maskedAadhaarMatch = upperText.match(/\b([X*]{4}[\s-]?[X*]{4}[\s-]?\d{4})\b/)
  const fullAadhaarMatch = rawText.match(/(?<!\d)([2-9]\d{3})[\s-]?(\d{4})[\s-]?(\d{4})(?!\d)/)
  if (
    !idType &&
    (maskedAadhaarMatch ||
      fullAadhaarMatch ||
      upperText.includes("AADHAAR") ||
      upperText.includes("UIDAI") ||
      upperText.includes("GOVERNMENT OF INDIA"))
  ) {
    idType = "MaskedAadhaar"
    if (fullAadhaarMatch) {
      idNumber = `${fullAadhaarMatch[1]}-${fullAadhaarMatch[2]}-${fullAadhaarMatch[3]}`
    } else if (maskedAadhaarMatch) {
      idNumber = maskedAadhaarMatch[1].replace(/[*]/g, "X").replace(/\s+/g, "-")
    }
  }

  // 6. Extract Name
  if (!fullName) {
    const dobIdx = lines.findIndex((l) =>
      /\b(dob|d\.o\.b|birth|yob|father|male|female)\b/i.test(l)
    )
    if (dobIdx > 0) {
      for (let i = dobIdx - 1; i >= Math.max(0, dobIdx - 3); i--) {
        if (isLikelyPersonName(lines[i])) {
          fullName = toTitleCase(cleanNameCandidate(lines[i]))
          break
        }
      }
    }

    if (!fullName) {
      const nameLabelIdx = lines.findIndex((l) => /^name\s*[:\-]/i.test(l))
      if (nameLabelIdx >= 0) {
        const inline = lines[nameLabelIdx].replace(/^name\s*[:\-]\s*/i, "").trim()
        if (isLikelyPersonName(inline)) {
          fullName = toTitleCase(cleanNameCandidate(inline))
        } else if (nameLabelIdx + 1 < lines.length && isLikelyPersonName(lines[nameLabelIdx + 1])) {
          fullName = toTitleCase(cleanNameCandidate(lines[nameLabelIdx + 1]))
        }
      }
    }

    if (!fullName) {
      const candidate = lines.find((l) => isLikelyPersonName(l))
      if (candidate) fullName = toTitleCase(cleanNameCandidate(candidate))
    }
  }

  // 7. Extract Mobile Number
  const mobileLabelMatch = rawText.match(/(?:mobile|mob|ph|phone)[^\d]{0,10}([6-9]\d{9})\b/i)
  if (mobileLabelMatch) {
    mobile = mobileLabelMatch[1]
  } else {
    const standaloneMob = rawText.match(/(?<![\d-])([6-9]\d{9})(?![\d-])/)
    if (standaloneMob) mobile = standaloneMob[1]
  }

  // 8. Extract Address (supports Aadhaar front/back & e-Aadhaar PDF)
  const addrStartIdx = lines.findIndex((l) =>
    /\b(address|c\/o|s\/o|d\/o|w\/o)\b/i.test(l) || /^to\s*[:,\-]?$/i.test(l)
  )
  const pinIdx = lines.findIndex((l) => /\b[1-9]\d{5}\b/.test(l))

  if (addrStartIdx >= 0) {
    const endIdx = pinIdx >= addrStartIdx ? pinIdx : Math.min(lines.length - 1, addrStartIdx + 5)
    const slice = lines
      .slice(addrStartIdx, endIdx + 1)
      .map((l) => l.replace(/^(?:address|to)\s*[:\-]?\s*/i, "").trim())
      .filter((l) => l.length > 1 && !/^\d{4}[\s-]?\d{4}[\s-]?\d{4}$/.test(l) && !isBoilerplateLine(l))
    const joined = slice.join(", ").replace(/,\s*,+/g, ", ").trim()
    if (joined.length >= 10) address = joined
  } else if (pinIdx > 0) {
    const startIdx = Math.max(0, pinIdx - 4)
    const slice = lines
      .slice(startIdx, pinIdx + 1)
      .filter((l) => !isBoilerplateLine(l) && !/^\d{4}[\s-]?\d{4}[\s-]?\d{4}$/.test(l))
    const joined = slice.join(", ").replace(/,\s*,+/g, ", ").trim()
    if (joined.length >= 10) address = joined
  }

  const extractedFields: string[] = []
  if (idType) extractedFields.push("ID Type")
  if (idNumber) extractedFields.push("ID Number")
  if (fullName) extractedFields.push("Full Name")
  if (mobile) extractedFields.push("Mobile")
  if (address) extractedFields.push("Address")

  return {
    idType,
    idNumber,
    fullName,
    address,
    mobile,
    extractedFields,
    rawText,
  }
}

export async function extractDetailsFromFiles(
  files: File[],
  pdfPasswords: Record<string, string> = {},
  onProgress?: (statusLabel: string, percent: number) => void
): Promise<ExtractedIdDetails> {
  const perFileResults: ExtractedIdDetails[] = []
  const rawTexts: string[] = []

  for (let i = 0; i < files.length; i++) {
    const file = files[i]
    const label = files.length > 1 ? `Scanning file ${i + 1} of ${files.length}...` : "Reading details from ID..."
    let text = ""

    if (file.type === "application/pdf" || file.name.toLowerCase().endsWith(".pdf")) {
      text = await extractTextFromPdf(file, pdfPasswords[file.name], (pct) => {
        if (onProgress) onProgress(label, pct)
      })
    } else if (file.type.startsWith("image/")) {
      const imageData = await preprocessImageForOcr(file)
      const result = await Tesseract.recognize(imageData, "eng", {
        logger: (m) => {
          if (m.status === "recognizing text" && typeof m.progress === "number" && onProgress) {
            onProgress(label, Math.round(m.progress * 100))
          }
        },
      })
      text = result.data.text || ""
    }

    if (text.trim()) {
      rawTexts.push(text)
      perFileResults.push(parseIndianIdText(text))
    }
  }

  const combinedText = rawTexts.join("\n")
  const combinedParsed = parseIndianIdText(combinedText)

  // Prefer explicit per-file hits if combined text loses context across pages
  const merged: ExtractedIdDetails = {
    idType: perFileResults.find((r) => r.idType)?.idType ?? combinedParsed.idType,
    idNumber: perFileResults.find((r) => r.idNumber)?.idNumber ?? combinedParsed.idNumber,
    fullName: perFileResults.find((r) => r.fullName)?.fullName ?? combinedParsed.fullName,
    mobile: perFileResults.find((r) => r.mobile)?.mobile ?? combinedParsed.mobile,
    address: perFileResults.find((r) => r.address)?.address ?? combinedParsed.address,
    extractedFields: [],
    rawText: combinedText,
  }

  if (merged.idType) merged.extractedFields.push("ID Type")
  if (merged.idNumber) merged.extractedFields.push("ID Number")
  if (merged.fullName) merged.extractedFields.push("Full Name")
  if (merged.mobile) merged.extractedFields.push("Mobile")
  if (merged.address) merged.extractedFields.push("Address")

  return merged
}
