import * as React from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";

import AppBar from "./views/common/AppBar";
import NotFoundPage from "./pages/404";
import LoadingPage from "./views/common/LoadingPage";
import AboutPage from "./pages/about";
import SettingPage from "./pages/setting";
import Center from "./views/center";
import Login from "./pages/login";
import Register from "./views/register";
import TimelinePage from "./views/timeline";
import Search from "./views/search";
import Admin from "./views/admin";
import AlertHost from "./views/common/alert/AlertHost";

export default function App() {
  return (
    <React.Suspense fallback={<LoadingPage />}>
      <BrowserRouter>
        <AppBar />
        <div style={{ height: 56 }} />
        <Routes>
          <Route path="center" element={<Center />} />
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="settings" element={<SettingPage />} />
          <Route path="about" element={<AboutPage />} />
          <Route path="search" element={<Search />} />
          <Route path="admin/*" element={<Admin />} />
          <Route path=":owner" element={<TimelinePage />} />
          <Route path=":owner/:timeline" element={<TimelinePage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
        <AlertHost />
      </BrowserRouter>
    </React.Suspense>
  );
}
