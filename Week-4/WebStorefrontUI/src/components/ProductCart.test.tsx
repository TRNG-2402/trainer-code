// This is my test file, notice it ends in .test.tsx AND it's little React nucleus is orange
// If we have vitest (or Jest) setup properly, it will recognize this file and run the tests inside. 
import { describe, it, expect } from "vitest";
import { render, screen } from '@testing-library/react';
import { MemoryRouter } from "react-router-dom";
import ProductCard from "./ProductCard"; // The thing we are testing here
import type { Product } from "../types/Product";

// We are going to follow more or less the same Arrange-Act-Assert pattern
// Starting with creating any mock objects. Note: mocks in React don't have to be 
// as "fleshed out" as say a mock repo layer object in C# using Moq library
// FOr example, we're going to create a mockProduct - just dummy data we can test against
const mockProduct: Product = {
    productId: 1,
    name: 'Thinkpad Laptop',
    description: 'X1 Carbon laptop',
    price: 1399.99,
    stock: 10,
    createdAt: '2026-01-15T00:00:00Z',
    categoryId: 1
}

// React-Testing-Library tests (whether Jest or Vitest) follow the exact same syntax and pattern.
// You "describe" the thing you're testing, you say what "it" does, and you tell the test what you "expect"
describe('<ProductCard />', () => {

    // The tests kind-of read like english language - pretty neat how self documenting they are
    // compared to tests in xUnit and jUnit
    it('renders the product name and price', ()  => {
        render(
            <MemoryRouter>
                <ProductCard product={mockProduct} />
            </MemoryRouter>
        );

        // Kind of like a console.log() but for your tests
        // Will spit out exactly what HTML is rendering inside of jest-dom
        screen.debug()

        expect(screen.getByText('Thinkpad Laptop')).toBeInTheDocument();
        expect(screen.getByText('$1399.99')).toBeInTheDocument();

    });

    it('shows "Out of stock" when the stock is 0', () => {
        // We only need this in one test, so we can just create it inside of the It() call
        const outOfStockProduct: Product = { ...mockProduct, stock: 0}

        // Render
        render(
            <MemoryRouter>
                <ProductCard product={outOfStockProduct} />
            </MemoryRouter>
        )

        // Assert
        expect(screen.getByText('Out of Stock')).toBeInTheDocument();
    });
});