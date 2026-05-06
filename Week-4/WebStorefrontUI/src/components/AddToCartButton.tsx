import type { Product } from "../types/Product"
import { useCart } from "../context/CartContext"

interface AddToCartButtonProps {
    product: Product
}

export default function AddToCartButton({ product }: AddToCartButtonProps) {
    // We just need the dispatch function - all this button does is call upon
    // our reducer
    const { dispatch } = useCart();

  return (
    <button
        onClick={() => dispatch({ type: 'ADD_ITEM', payload: product})}
        disabled={product.stock === 0} 
    >
        {product.stock === 0 ? 'Out of stock' : 'Add to cart'}
    </button>
  )
}
