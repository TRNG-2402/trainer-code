import './App.css'
import ProductCard from './components/ProductCard' // Bringing in our ProductCard component
import type { Product } from './types/Product' // Bringing in our Product
import SearchBar from './components/SearchBar';
import { useState } from 'react'; // Bringing in our State hook
import EmptyState from './components/EmptyState';

// For now, in lieu of making actual API calls
// We're going to hardcode a list of products
// Once we can ask our API for the real list that's in the DB we'll stop using this list

const products: Product[] = [
  {
    productId : 1,
    name: 'Thinkpad Charger',
    description: 'USB-C Charger',
    price: 40.00,
    stock: 10,
    createdAt: '2026-01-15',
    categoryId: 1
  },
  {
    productId : 2,
    name: 'Mechanical Keyboard',
    description: 'MX Red Switch, usb-c',
    price: 140.00,
    stock: 2,
    createdAt: '2026-01-15',
    categoryId: 1
  },
  {
    productId : 3,
    name: 'Standing desk mat',
    description: null,
    price: 30.00,
    stock: 0,
    createdAt: '2026-01-15',
    categoryId: 3
  }
];

function App() {

  // Lets add State to our App
  // We will use state to hold user input from a searchbar
  // UseState needs us to provide a Tuple
  // whatToHold, Setter - we can also provide some initial value (if we need to) inside the useState function call.
  const [searchTerm, setSearchTerm] = useState('');

  // We want to break out the filtering outside of the return
  // mostly to keep to keep things clean/readable
  const filtered = products.filter((p) => 
    p.name.toLowerCase().includes(searchTerm.toLowerCase())
  );
  
  return (
    <main>
      <h1>WebStoreFront</h1>
      <SearchBar value={searchTerm} onSearchChange={setSearchTerm} />
      {/* Using a ternary to render separate sets of components/data based on some condition */}
      {
        filtered.length === 0 ? (
          <EmptyState message={`No products match "${searchTerm}`} />
        ) : (
          <section>
            {/* For every product in the list, we need to render a ProductCard */}

            {// Map is a function that iterates over an array, and for each element in the array,
            // it does "something" - we need to give it a function to apply or run for each product object
            // that we refer to as p in this case. React also needs us to pass in a key, or it'll complain
            // because otherwise it can't make sure its not rendering duplicates.
              filtered.map((p) => (
                <ProductCard key={p.productId} product={p}/>
              ))}
          </section>
        )}
    </main>
  )
}

export default App
