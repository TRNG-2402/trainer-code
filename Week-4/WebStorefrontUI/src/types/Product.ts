// Mirrors the shape returned by GET /api/Product on our backend
// IF we change what that endpoint returns, we MUST come back and update this file

export interface Product {
    productId: number,
    name: string,
    description: string | null, 
    price: number,
    stock: number,
    createdAt: string, // IF we need to parse this into a Date object for whatever reason, we do so later
    categoryId: number
}