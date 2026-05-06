import { useEffect, useState } from 'react'
import { productService } from '../services/productService'
import { Link, useParams } from 'react-router-dom'
import type { Product } from '../types/Product'; // Bringing in our Product type
import AddToCartButton from '../components/AddToCartButton';

export default function ProductDetail() {

    // First, grabbing the productId from the route itself via useParams
    const { productId } = useParams<{ productId: string }>();

    //Storing some stuff in state - first the product we get back from the API
    const [ product, setProduct ] = useState<Product | null>(null);

    // Loading for conditional render during load state
    const [ isLoading, setIsLoading ] = useState(true);

    // We want to grab our product info when we land on this page
    // Note: We are doing this BECAUSE we want the use to be able to navigate to this product
    // from anywhere via the browser URL. 
    useEffect(() => {
        // If we land on this component without a productId somehow, fail fast
        if(!productId) return;

        productService
            .getById(Number(productId)) //call getById, pass in the productId from useParams (the route)
            .then((p) => setProduct(p))
            .finally(() => setIsLoading(false));


    }, [productId]);

    // Conditional rendering for product loading and not found states
    if (isLoading) return <p>Loading...</p>
    if (!product) return <p>Product not found.</p>


  return (
    <article>
        <h2>{product.name}</h2>
        <p>{product.description ?? 'No description.'}</p>
        <p>Price: <strong>${product.price.toFixed(2)}</strong></p>
        <p>Stock: {product.stock}</p>
        <AddToCartButton product={product} />
        <Link to="/products"> Back to products</Link>
    </article>
  )
}
