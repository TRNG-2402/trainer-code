import { api } from "./api";
import type { Product } from "../types/Product";

export const productService = {
    // Product service's whole job is to hold functions that I can call upon when needed
    // to send requests to my Product endpoints on my api - WITHOUT having to construct
    // axios objects all over the place

    // Sends GET to http://localhost:5247/api/Product 
    async getAllProducts(): Promise<Product[]> {
        const response = await api.get<Product[]>('/Product');
        return response.data; // returning the body of the response, because response also has
        // things like the return headers, auth stuff, etc. 
    }
}