import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import App from "./App";

import "./styles/base.css";
import "./styles/layout.css";
import "./styles/forms.css";
import "./styles/messages.css";
import "./styles/games.css";
import "./styles/locations.css";
import "./styles/modals.css";
import "./styles/responsive.css";

import { UserProvider } from "./context/UserContext";

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
      <UserProvider>
        <App />
      </UserProvider>
    </BrowserRouter>
  </React.StrictMode>
);