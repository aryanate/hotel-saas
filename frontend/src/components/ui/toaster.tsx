import { useToast } from "./use-toast"
import { Toast } from "./toast"

export function Toaster() {
  const { toasts, dismiss } = useToast()

  return (
    <div className="fixed bottom-0 right-0 z-50 flex max-h-screen w-full flex-col-reverse p-4 sm:bottom-0 sm:right-0 sm:top-auto sm:flex-col md:max-w-[420px] gap-2">
      {toasts.map((t) => (
        <Toast key={t.id} variant={t.variant} onClose={() => dismiss(t.id)}>
          {t.title && <div className="text-sm font-semibold">{t.title}</div>}
          {t.description && (
            <div className="text-sm opacity-90">{t.description}</div>
          )}
          {t.action}
        </Toast>
      ))}
    </div>
  )
}
