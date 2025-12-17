import { HashRouter as Router, Routes, Route } from 'react-router-dom';
import { SecurityContext } from './types/Security';
import { ModuleContext } from './types/Module';
import ContactList from './pages/ContactList';
import ContactForm from './pages/ContactForm';

interface AppProps {
  security: SecurityContext;
  moduleContext: ModuleContext;
}

export default function App({ security, moduleContext }: AppProps) {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<ContactList security={security} moduleContext={moduleContext} />} />
        <Route path="/add" element={<ContactForm moduleContext={moduleContext} />} />
        <Route path="/edit/:id" element={<ContactForm moduleContext={moduleContext} />} />
      </Routes>
    </Router>
  );
}

