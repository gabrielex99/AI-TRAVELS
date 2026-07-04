export interface ActivityOption {
  title: string;
  description: string;
  latitude: number;
  longitude: number;
  gyg_search_term: string;
}

export interface TimeSlot {
  option_a: ActivityOption;
  option_b: ActivityOption;
}

export interface DaySlots {
  morning: TimeSlot;
  afternoon: TimeSlot;
  evening: TimeSlot;
}

export interface DayPlan {
  day: number;
  theme: string;
  slots: DaySlots;
}

export interface FlightWidgetParams {
  destination_iata: string;
  suggested_months: string[];
}

export interface ItineraryResponse {
  destination: string;
  total_days: number;
  flight_widget_params: FlightWidgetParams;
  itinerary: DayPlan[];
}

export interface EnrichedItineraryResponse {
  id?: string;
  destination: string;
  total_days: number;
  flight_widget_params: FlightWidgetParams;
  itinerary: DayPlan[];
  affiliate_links: Record<string, string>;
  budget_max?: number;
  num_people?: number;
  start_date?: string;
  end_date?: string;
}

export interface TripRequest {
  destination: string;
  start_date: string;
  end_date: string;
  budget_max: number;
  num_people: number;
}
