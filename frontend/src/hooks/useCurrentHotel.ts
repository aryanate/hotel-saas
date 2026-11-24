import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { hotelService } from "@/services/hotel.service"
import { useAuth } from "@/hooks/useAuth"
import type { Hotel } from "@/types"

export const HOTEL_QUERY_KEY = ["current-hotel"]

export function useCurrentHotel() {
  const queryClient = useQueryClient()
  const { user } = useAuth()

  const query = useQuery({
    queryKey: [...HOTEL_QUERY_KEY, user?.hotelId],
    queryFn: async () => {
      const response = await hotelService.getMyHotel()
      return response.data || null
    },
    enabled: !!user?.hotelId,
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
