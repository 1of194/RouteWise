export interface IRoute {
    name: string;
    priority: boolean;
}

export enum PriorityLevel {
    Start = 0,     // starting location
    High = 1,      // IsPriority = true
    Normal = 2     // IsPriority = false
}

export type GeoLocation = {
    id: number;
    latitude: number;
    longitude: number;
  };
  
  
  export type DeliveryAddresses = {
    id?: number;
    street: string;
    city: string;
    postalCode: string;
    priority: PriorityLevel;
    geolocationId: number;
    geolocation?: GeoLocation;
    createdAt: string; 
  };

export type BookmarkType = {
  id: number;
  userId: number;
  deliveryaddresses: DeliveryAddresses[];
  geolocations: Geolocation[];
}


  export type BookmarkResponse = {
  message: string;
  bookmark: BookmarkType[];
};

export type DeliveryAddressResponse = 
  | { message: string; deliveryAddresses?: DeliveryAddresses[] }
  | DeliveryAddresses[];