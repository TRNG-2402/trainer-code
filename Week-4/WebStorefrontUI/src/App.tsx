import { Route, Routes } from 'react-router-dom'
import './App.css'
import Home from './pages/Home'
import Product from './pages/Product'
import Categories from './pages/Categories'
import Login from './pages/Login'
import ProductDetail from './pages/ProductDetail'
import NavBar from './components/NavBar'
import NotFound from './pages/NotFound'
import CategoryProducts from './pages/CategoryProducts'
import ProtectedRoute from './components/ProtectedRoute'
import Cart from './pages/Cart'


// For now, in lieu of making actual API calls
// We're going to hardcode a list of products
// Once we can ask our API for the real list that's in the DB we'll stop using this list

// const products: Product[] = [
//   {
//     productId : 1,
//     name: 'Thinkpad Charger',
//     description: 'USB-C Charger',
//     price: 40.00,
//     stock: 10,
//     createdAt: '2026-01-15',
//     categoryId: 1
//   },
//   {
//     productId : 2,
//     name: 'Mechanical Keyboard',
//     description: 'MX Red Switch, usb-c',
//     price: 140.00,
//     stock: 2,
//     createdAt: '2026-01-15',
//     categoryId: 1
//   },
//   {
//     productId : 3,
//     name: 'Standing desk mat',
//     description: null,
//     price: 30.00,
//     stock: 0,
//     createdAt: '2026-01-15',
//     categoryId: 3
//   }
// ];

function App() {

  return(
    <main>
      <h1>WebStoreFront</h1>
      {/* We will have a navbar coming soon, for now, routes below*/}
      <NavBar />
      <Routes>
        <Route path='/' element={<Home />} />
        <Route path='/products' element={<Product />} />


        <Route 
          path='/products/:productId' 
          element={
            <ProtectedRoute>
              <ProductDetail />
            </ProtectedRoute>
          } 
        />


        <Route 
          path='/categories' 
          element={
            <ProtectedRoute>
              <Categories />
            </ProtectedRoute>
          } 
        />


        <Route 
          path='/categories/:categoryId/products' 
          element={
            <ProtectedRoute>
              <CategoryProducts />
            </ProtectedRoute>            
          }
        />

        <Route path='/cart' element={<Cart />} />
        <Route path='/login' element={<Login />} />
        <Route path='*' element={<NotFound />} />
      </Routes>
    </main>
  )
}

export default App
