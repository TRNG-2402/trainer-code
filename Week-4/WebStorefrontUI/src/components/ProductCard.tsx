import React from 'react'
import type { Product } from '../types/Product'
import styles from './ProductCard.module.css'

// Because this is typescript we need to give our "arguments" to our function components,
// in React referred to as props, a type
interface ProductCardProps {
    product: Product;
}

// Components can share state with their children via Props
// Props are like arguments to our function components
// They use a different syntax than just passing in an object as an argument in a TS function
export default function ProductCard({ product }: ProductCardProps) {


    // The return of a React function component is... things to render. You can think of JSX/TSX
    // as an extension of HTML that lets you call upon TS code inside of it, as needed
  return (
    <div className={styles.card}>
        <h3>{product.name}</h3>
        {/* 
            This is a comment in my TSX return - kinda extra but we gotta it this way. 
            product.description COULD be null - we have no way of knowing if it is
            Thankfully, React is very good at handling this sort of conditional rendering
            We can use things like Null Coalescing operators or Ternary operators 
            to easily render based on conditions being met or not met, avoiding long if/elses 
            Note: These come from TS/JS not React itself
        */}
        <p>{product.description ?? 'No description'}</p> {/* Null Coalescing operator.*/}
        <p><strong>${product.price.toFixed(2)}</strong></p>
        {/* Leveraging conditional rendering to display "Out Of Stock" message
            if the value of product.stock is zero.
            Below we use an in-line if expression (coming from JS/TS)
            condition ? thing-if-true : thing-if-false
            */}
        <p className={product.stock === 0 ? styles.outOfStock : undefined}>
            {product.stock > 0 ? `${product.stock}` : 'Out of Stock'}
        </p>
    </div>
  )
}
