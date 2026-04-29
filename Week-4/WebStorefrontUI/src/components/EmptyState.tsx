import React from 'react'

// We will take in some props for this, so we have to type them
interface EmptyStateProps {
    message: string
}

export default function EmptyState( {message} : EmptyStateProps) {
  return (
    <p style={{ textAlign: 'center', color: '#888', padding: '2rem'}}>
        {message}
    </p>
  )
}
