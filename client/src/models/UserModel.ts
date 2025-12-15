export type User = {
    userId : Number,
    userName: String

}

export enum PageType {
    'Login' = 1,
    'Register' = 2
}

export type LoginResponse = {
    Message : string,
    accessToken: string,  
    
}