import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { hotelService } from "@/services/hotel.service"
import type { Hotel } from "@/types"

export const HOTEL_QUERY_KEY = ["current-hotel"]

export function useCurrentHotel() {
  const queryClient = useQueryClient()

  const query = useQuery({
    queryKey: HOTEL_QUERY_KEY,
    queryFn: async () => {
      const response = await hotelService.getMyHotel()
      return response.data || null
    },
    staleTime: 1000 * 60 * 5, // 5 minutes
  })

  const updateMutation = useMutation({
    mutationFn: async (payload: Partial<Hotel>) => {
      const res = await hotelService.updateHotel(payload)
      return res.data
    },
    onSuccess: (updated) => {
      queryClient.setQueryData(HOTEL_QUERY_KEY, updated)
    },
  })

  return {
    hotel: query.data,
    isLoading: query.isLoading,
    error: query.error,
    refetch: query.refetch,
    updateHotel: updateMutation.mutateAsync,
    isUpdating: updateMutation.isPending,
  }
}
