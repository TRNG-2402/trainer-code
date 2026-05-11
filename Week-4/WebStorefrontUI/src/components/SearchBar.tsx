
import styles from './SearchBar.module.css'

// We can pass in anything as props
// ANYTHING
// Any object - methods are objects in JS/TS
// We can pass info from parent -> child as props
// And a child can send info back up to its parent, by taking in a state setter function
// and calling it inside the child, to update the state stored in the parent 
interface SearchBarProps {
    value: string; // place to hold the value the user types in
    onSearchChange: (next: string) => void; //the setter for the state held in App.tsx
}

export default function SearchBar({ value, onSearchChange} : SearchBarProps) {
  return (
    <input
        type='text'
        placeholder='Search products...'
        value={value}
        onChange={ (e) => onSearchChange(e.target.value)} // When someone types a new value, call the setter with that value
        className={styles.search}
    />
  )
}
