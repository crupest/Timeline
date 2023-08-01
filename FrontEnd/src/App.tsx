import { Suspense } from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";

import AppBar from "./views/common/AppBar";
import NotFoundPage from "./pages/404";
import HomePage from "./pages/home";
import AboutPage from "./pages/about";
import SettingPage from "./pages/setting";
import LoginPage from "./pages/login";
import RegisterPage from "./pages/register";
import TimelinePage from "./pages/timeline";
import LoadingPage from "./pages/loading";
import AlertHost from "./views/common/alert/AlertHost";

export default function App() {
  return (
    <Suspense fallback={<LoadingPage />}>
      <BrowserRouter>
        <AppBar />
        <div style={{ height: 56 }} />
        <Routes>
          <Route path="login" element={<LoginPage />} />
          <Route path="register" element={<RegisterPage />} />
          <Route path="settings" element={<SettingPage />} />
          <Route path="about" element={<AboutPage />} />
          <Route path=":owner" element={<TimelinePage />} />
          <Route path=":owner/:timeline" element={<TimelinePage />} />
          <Route path="" element={<HomePage />} />
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
        <AlertHost />
      </BrowserRouter>
    </Suspense>
  );
}
