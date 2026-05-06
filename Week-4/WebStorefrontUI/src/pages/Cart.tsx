import { useCart } from "../context/CartContext"

export default function Cart() {

    // Grabbing what we need via useCart()
    const { state, dispatch } = useCart();

    // As a general rule, we want state to be as small as possible. That doesn't mean we want to 
    // cut out necessary items - but there are things that don't really ever need to be in state
    // As a GENERAL rule: if we can derive some value from what's already in state, it doesn't need to
    // be held there. For our example: total price. 

    // This will be computed every re-render, and thats fine. its quick math
    const total = state.items.reduce(
        (sum, item) => sum + (item.product.price * item.quantity),
        0 // starting value of sum 
    );

    // Check if cart is empty
    if( state.items.length === 0 ) {
        // Rendering a message to the user, they have nothing in their cart
        return (
            <>
                <h2>Cart</h2>
                <p>Your cart is empty.</p>
            </>
        );
    }

    // if the cart is not empty, then we render the "full" return for the user,
    // displaying the items in their cart, the quantity of each, and the sum total of the items
    // in their cart
    return (
        <>
            <h2>Cart</h2>
            <ul>
                {state.items.map((item) => (
                    <li key={item.product.productId}>
                        <strong>{item.product.name}</strong>
                        {' ----- '}
                        <input
                            type="number"
                            min={1}
                            value={item.quantity}
                            onChange={(e) => 
                                dispatch({
                                    type: 'UPDATE_QUANTITY',
                                    payload: {
                                        productId: item.product.productId,
                                        quantity: Number(e.target.value)
                                    }
                                })
                            }
                            style={{ width: '4rem'}}
                        />
                        {' x $'}
                        {item.product.price.toFixed(2)}
                        {' = $'}
                        {(item.product.price * item.quantity).toFixed(2)}
                        <button
                            onClick={() => 
                                dispatch({
                                    type: 'REMOVE_ITEM',
                                    payload: {productId: item.product.productId}
                                })
                            }
                            style={{ marginLeft: '0.5rem'}}
                        >
                            Remove
                        </button>
                    </li>
                ))}
            </ul>
            <p>
                <strong>Total: ${total.toFixed(2)}</strong>
            </p>
            <button
                onClick={() => 
                    dispatch({type: 'CLEAR'})
                }
            >
                Clear cart
            </button>
        </>
    )
}

