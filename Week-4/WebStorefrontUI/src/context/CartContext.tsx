import { type CartState, type CartAction, initialCartState } from "../types/Cart";
import { createContext, useContext, useReducer, useMemo, type ReactNode, type Dispatch } from "react";

// Reducers are pure functions
// Given the same initial state and action, we will ALWAYS end up at the same new state
// we never directly mutate the input state -> we simply return a new valid permutation of state
// we never call anything that can be considered a side effect (no db calls, no HTTP requests via axios or fetch)
//
// STUDENT NOTE: Same Fast Refresh rule that fired on useAuth fires here too — `cartReducer` is a
// plain function, not a component. We export it (rather than keeping it file-local) so unit tests
// can import and test the reducer in isolation, which is one of the main reasons reducers are
// nice — they're pure functions that are trivial to test without rendering React.
// eslint-disable-next-line react-refresh/only-export-components
export function cartReducer(state: CartState, action: CartAction) {

    // Reducers are like a switch for your state - we define the possible
    // actions a user can take - as well as the possible new state configuration coming out of that action
    // also, we use a switch to do this
    switch (action.type) {
        
        // Our first case is adding an item. We probably (to conform to user expectations
        // on our a cart works) want to check if the item is already in the cart. If its not,
        // add it to the cart. if it is, then simply update the quantity. 
        case 'ADD_ITEM': {
            // Check to see if there is an item in the cart with the same productId as the 
            // one we're trying to add
            const existing = state.items.find(
                (item) => item.product.productId === action.payload.productId
            );

            // If it is already in the cart, simply update the quantity.
            if (existing) {
                return {
                    // First, take the state object
                    // unpack it via the spread operator
                    ...state,
                    items: state.items.map((item) => // then look at it's list of items
                        // for every item in the list we want to...
                        // check to see if it matches the item we sent in to add (via productId)
                        item.product.productId === action.payload.productId 
                            ? { ...item, quantity: item.quantity + 1} // if it matches, add 1 to its quantity
                            : item // if it doesn't, simply preserve the item (productId and quantity) as is
                        )
                }
            }

            // If it's not already in the cart, we add it to the cart 
            return {
                ...state, // Unpack state
                // Update it's items list, preserving what's in there already
                // and appending a new item with a quantity of 1 to the list
                items: [...state.items, { product: action.payload, quantity: 1}]
            }
        }

        case 'REMOVE_ITEM': 
            return {
                // Spread our state object out
                ...state,
                // Look inside it's items list and filter
                // If an item doesn't match the productId we're trying to remove from the cart
                // then it makes it through the filter
                items: state.items.filter(
                    (item) => item.product.productId !== action.payload.productId
                )
            };
        
        case 'UPDATE_QUANTITY':
            // For UPDATE_QUANTITY we can choose to treat a negative or a 0 
            // as a removal 
            if (action.payload.quantity <= 0) {
                // We already have the logic to remove an item, so we can least resistance this
                // by recursively calling our cartReducer and calling the 'REMOVE_ITEM' case
                return cartReducer(state, {
                    type: 'REMOVE_ITEM',
                    payload: { productId: action.payload.productId }
                });
            }

            // Otherwise, if they pass in a non-zero or non-negative value
            // we can process the update
            return {
                ...state,
                items: state.items.map((item) => 
                    item.product.productId === action.payload.productId
                        ? {...item, quantity: action.payload.quantity}
                        : item
                )
            };
        
        case 'CLEAR':
            return initialCartState;

        default: {
            // Like any switch we need a default case. We can actually have TS enforce
            // that if somebody adds a new potential action to the CartAction Type, it MUST
            // be handled here. And TS will enforce this for us
            //
            // STUDENT NOTE: We previously used `const _exhaustive: never = action;` paired with a
            // `@ts-ignore` to do this exhaustiveness check. That tripped two eslint rules:
            //   1. `@ts-ignore` should be `@ts-expect-error` (and `@ts-expect-error` itself errors
            //      if the next line has no TS error to suppress — fragile).
            //   2. `_exhaustive` was an unused variable.
            // The modern pattern is `satisfies never` (TS 4.9+): if every CartAction variant is
            // handled in the cases above, TypeScript narrows `action` to `never` here and the
            // `satisfies` passes. If someone adds a new variant to CartAction without a case for
            // it, `action` will NOT narrow to `never` and TS will flag this line at compile time.
            // Same exhaustiveness guarantee, no dummy variable, no `@ts-*` directive needed.
            action satisfies never;
            return state;
        }
    }

}

// Lets create an interface for the CartContextValue
interface CartContextValue {
    state: CartState,
    dispatch: Dispatch<CartAction>;
}

// We need to create our context, just like AuthContext
const CartContext = createContext<CartContextValue | undefined>(undefined);

// We need to write our provider function
export function CartProvider({ children }: { children: ReactNode }) {

    // Notice we aren't importing useState, because we're offloading the management of that state
    // to our reducer - we need to call useReducer with 2 things: which reducer function, and an initial state value
    const [state, dispatch] = useReducer(cartReducer, initialCartState)

    // Using useMemo for the same reason here as we did so in AuthContext - to save on React re-renders
    // more performant. Not creating and discarding a ton of objects for no reason. 
    const value = useMemo(() => ({state, dispatch}), [state]);

    return <CartContext.Provider value={value}>{children}</CartContext.Provider>
}

// Finally, like any context, we need a custom hook
//
// STUDENT NOTE: Same Fast Refresh rule as useAuth — see the note in AuthContext.tsx for the full
// explanation. Short version: this hook is a non-component export in a .tsx file, which breaks
// React Fast Refresh's ability to preserve component state across hot reloads. We accept the
// trade-off to keep provider + hook colocated for teaching.
// eslint-disable-next-line react-refresh/only-export-components
export function useCart(): CartContextValue {
    // Our custom hook to call upon our context is going to look basically identical to useAuth()
    const ctx = useContext(CartContext);
    
    if (ctx === undefined) {
        throw new Error('useCart hook MUST be used inside of an <CartProvider>')
    }

    return ctx;

}