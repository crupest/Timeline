import React from "react";
import { useTranslation, Trans } from "react-i18next";

import AppBar from "../common/AppBar";

import authorAvatarUrl from "./author-avatar.png";
import githubLogoUrl from "./github.png";

const frontendCredits: {
  name: string;
  url: string;
}[] = [
  {
    name: "reactjs",
    url: "https://reactjs.org",
  },
  {
    name: "typescript",
    url: "https://www.typescriptlang.org",
  },
  {
    name: "bootstrap",
    url: "https://getbootstrap.com",
  },
  {
    name: "react-bootstrap",
    url: "https://react-bootstrap.github.io",
  },
  {
    name: "babeljs",
    url: "https://babeljs.io",
  },
  {
    name: "webpack",
    url: "https://webpack.js.org",
  },
  {
    name: "sass",
    url: "https://sass-lang.com",
  },
  {
    name: "eslint",
    url: "https://eslint.org",
  },
  {
    name: "prettier",
    url: "https://prettier.io",
  },
  {
    name: "pepjs",
    url: "https://github.com/jquery/PEP",
  },
  {
    name: "react-inlinesvg",
    url: "https://github.com/gilbarbara/react-inlinesvg",
  },
];

const backendCredits: {
  name: string;
  url: string;
}[] = [
  {
    name: "ASP.NET Core",
    url: "https://dotnet.microsoft.com/learn/aspnet/what-is-aspnet-core",
  },
  { name: "sqlite", url: "https://sqlite.org" },
  {
    name: "ImageSharp",
    url: "https://github.com/SixLabors/ImageSharp",
  },
];

const AboutPage: React.FC = () => {
  const { t } = useTranslation();

  return (
    <>
      <AppBar />
      <div className="mt-appbar px-2 mb-4">
        <div className="container mt-4 py-3 shadow border border-primary rounded bg-light">
          <h4 id="author-info">{t("about.author.title")}</h4>
          <div>
            <div className="d-flex">
              <img
                src={authorAvatarUrl}
                className="align-self-start avatar large rounded-circle"
              />
              <div>
                <p>
                  <small>{t("about.author.fullname")}</small>
                  <span className="text-primary">杨宇千</span>
                </p>
                <p>
                  <small>{t("about.author.nickname")}</small>
                  <span className="text-primary">crupest</span>
                </p>
                <p>
                  <small>{t("about.author.introduction")}</small>
                  {t("about.author.introductionContent")}
                </p>
              </div>
            </div>
            <p>
              <small>{t("about.author.links")}</small>
              <a
                href="https://github.com/crupest"
                target="_blank"
                rel="noopener noreferrer"
              >
                <img
                  src={githubLogoUrl}
                  className="about-link-icon text-body"
                />
              </a>
            </p>
          </div>
        </div>
        <div className="container mt-4 py-3 shadow border border-primary rounded bg-light">
          <h4>{t("about.site.title")}</h4>
          <p>
            <Trans i18nKey="about.site.content">
              0<span className="text-primary">1</span>2<b>3</b>4
              <a href="#author-info">5</a>6
            </Trans>
          </p>
          <p>
            <a
              href="https://github.com/crupest/Timeline"
              target="_blank"
              rel="noopener noreferrer"
            >
              {t("about.site.repo")}
            </a>
          </p>
        </div>
        <div className="container mt-4 py-3 shadow border border-primary rounded bg-light">
          <h4>{t("about.credits.title")}</h4>
          <p>{t("about.credits.content")}</p>
          <p>{t("about.credits.frontend")}</p>
          <ul>
            {frontendCredits.map((item, index) => {
              return (
                <li key={index}>
                  <a href={item.url} target="_blank" rel="noopener noreferrer">
                    {item.name}
                  </a>
                </li>
              );
            })}
            <li>...</li>
          </ul>
          <p>{t("about.credits.backend")}</p>
          <ul>
            {backendCredits.map((item, index) => {
              return (
                <li key={index}>
                  <a href={item.url} target="_blank" rel="noopener noreferrer">
                    {item.name}
                  </a>
                </li>
              );
            })}
            <li>...</li>
          </ul>
        </div>
      </div>
    </>
  );
};

export default AboutPage;
