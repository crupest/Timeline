import React from "react";
import { BrowserRouter as Router, Route, Switch } from "react-router-dom";

import AppBar from "./views/common/AppBar";
import LoadingPage from "./views/common/LoadingPage";
import Home from "./views/home";
import Login from "./views/login";
import Settings from "./views/settings";
import About from "./views/about";
import User from "./views/user";
import TimelinePage from "./views/timeline";
import AlertHost from "./views/common/alert/AlertHost";

import { dataStorage } from "./services/common";
import { userService, useRawUser } from "./services/user";

const NoMatch: React.FC = () => {
  return <div>Ah-oh, 404!</div>;
};

const LazyAdmin = React.lazy(
  () => import(/* webpackChunkName: "admin" */ "./views/admin/Admin")
);

const App: React.FC = () => {
  const [loading, setLoading] = React.useState<boolean>(true);

  const user = useRawUser();

  React.useEffect(() => {
    void userService.checkLoginState();
    void dataStorage.ready().then(() => setLoading(false));
  }, []);

  if (user === undefined || loading) {
    return <LoadingPage />;
  } else {
    return (
      <React.Suspense fallback={<LoadingPage />}>
        <Router>
          <AppBar />
          <Switch>
            <Route exact path="/">
              <Home />
            </Route>
            <Route exact path="/login">
              <Login />
            </Route>
            <Route path="/settings">
              <Settings />
            </Route>
            <Route path="/about">
              <About />
            </Route>
            <Route path="/timelines/:name">
              <TimelinePage />
            </Route>
            <Route path="/users/:username">
              <User />
            </Route>
            {user && user.administrator && (
              <Route path="/admin">
                <LazyAdmin user={user} />
              </Route>
            )}
            <Route>
              <NoMatch />
            </Route>
          </Switch>
          <AlertHost />
        </Router>
      </React.Suspense>
    );
  }
};

export default App;
