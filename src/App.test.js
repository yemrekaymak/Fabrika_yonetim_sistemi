import { render, screen } from '@testing-library/react';
import App from 'App';

test('renders login screen', () => {
  render(<App />);
  const heading = screen.getByText(/Fabrika YS Giriş/i);
  expect(heading).toBeInTheDocument();
});
