import { useEffect, useState } from "react"
import { Link } from "react-router-dom"
import { categoryService } from "../services/categoryService"
import type { Category } from "../types/Category"
import { useAuth } from "../context/AuthContext"

// Analogous to our Product.tsx page, but for categories. We will flesh this out a little less
// in the interest of time.
export default function Categories() {

  // useState to hold that categories list, for rendering
  const [categories, setCategories] = useState<Category[]>([]);

  // Asking for the user object in order to render the delete button if they're an admin
  const { user } = useAuth();

  // Just like our Products page, when this mounts we need to fire off an HTTP request.
  useEffect(() => {
    // Call our getAllCategories method, once promise resolves pipe the list of categories 
    // right into the setCategories method to update state - and trigger a render with the correct info
    categoryService.getAllCategories().then(setCategories);
  }, []);

  const handleDelete = async (id: number) => {
    // Call our categoryService delete method
    await categoryService.deleteById(id);
    // We could just call the same endpoint as useEffect - but that triggers an actual API call
    // so what we'll do is just filter out what we just deleted. if the user navigates back here
    // the component re-mounts, and they get the "correct" list
    setCategories((prev) => prev.filter((c) => c.categoryId !== id));
  }

  return (
    <>
      <h2>Categories</h2>
      <ul>
        {
          /* 
            Whether we're rendering entire components based on the contents of a list
            or simply <li> elements inside our unordered list (<ul>) - we can always use a map
          */
          categories.map((c) => (
            // We're going to play pretend for a moment - pretend I'd implemented the functionality on our API
            // to allow a user to ask for all the products that belong to a certain category
            // Something like a GET to 'api/categories/{category.id}/products'
            // For this demo, we'll create a component to "mock" render for that info, 
            // and so we'll mirror that structure on our route TO that component
            <li key={c.categoryId}>
              <Link to={`/categories/${c.categoryId}/products`} >{c.name}</Link>
              {/* Same "check condition, if true then render" pattern we've seen a few times up to now*/}
              {user?.role === 'Admin' && (
                <button
                  onClick={() => handleDelete(c.categoryId)}
                  style={{ marginLeft: '0.5rem'}} 
                >
                  Delete
                </button>
              )}
            </li>
          ))}
      </ul>
    </>
  )
}
