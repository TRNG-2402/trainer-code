import { useParams, Link } from "react-router-dom";


export default function CategoryProducts() {

    // Grabbing the categoryId from the route via useParams - just like our ProductDetail component
    const { categoryId } = useParams<{categoryId: string}>();

    return (
        <>
            <h2>Products in category {categoryId}</h2>
            {/* In our "real"/complete app, we'd fetch the products filtered by category
              i.e. list every product inside of category 1, 2, etc. We'd need to create a corresponding
                CategoryController method and flesh out the different layers of our API to do this
                and perhaps we could reuse the ProductCard components in here, and just render those*/}
            <p>(Filter goes here - fetching this info left as a TODO for now.)</p>
            <Link to="/categories">Back to categories</Link>
        </>
    );
}

