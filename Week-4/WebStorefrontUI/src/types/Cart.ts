import type { Product } from "./Product";


// CartItem or LineItem
export interface CartItem {
    product: Product;
    quantity: number;
}

// When we hold the state of a user's cart, for display in the front end
// what are we storing?
export interface CartState {
    items: CartItem[];
}

// Lets use a discriminated union type to hold our possible Cart Actions
// We want to hardcode what a user can do - and then we can call on these actions
// rather than trying to remember what changes we need to make for each one 
export type CartAction = 
    | { type: 'ADD_ITEM'; payload: Product}
    | { type: 'REMOVE_ITEM'; payload: { productId: number }}
    | { type: 'UPDATE_QUANTITY'; payload: { productId: number; quantity: number } }
    | { type: 'CLEAR' }

// This may be most useful for testing - coming soon! 
export const initialCartState: CartState = {
    items: []
}