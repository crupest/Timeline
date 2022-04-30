import React, { ReactElement } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";

import AppBar from "./views/common/AppBar";
import LoadingPage from "./views/common/LoadingPage";
import Center from "./views/center";
import Home from "./views/home";
import Login from "./views/login";
import Register from "./views/register";
import Settings from "./views/settings";
import About from "./views/about";
import TimelinePage from "./views/timeline";
import Search from "./views/search";
import Admin from "./views/admin";
import AlertHost from "./views/common/alert/AlertHost";

import { useUser } from "./services/user";

const NoMatch: React.FC = () => {
  return <div>Ah-oh, 404!</div>;
};

function App(): ReactElement | null {
  const user = useUser();

  return (
    <React.Suspense fallback={<LoadingPage />}>
      <BrowserRouter>
        <AppBar />
        <div style={{ height: 56 }} />
        <Routes>
          <Route index element={user == null ? <Home /> : <Center />} />
          <Route path="/home" element={<Home />} />
          <Route path="/center" element={<Center />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/settings" element={<Settings />} />
          <Route path="/about" element={<About />} />
          <Route path="/search" element={<Search />} />
          <Route path="/admin" element={<Admin />} />
          <Route path="/:owner" element={<TimelinePage />} />
          <Route path="/:owner/:timeline" element={<TimelinePage />} />
          <Route element={<NoMatch />} />
        </Routes>
        <AlertHost />
      </BrowserRouter>
    </React.Suspense>
  );
}

export default App;
