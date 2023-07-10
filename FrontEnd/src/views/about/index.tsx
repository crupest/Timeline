import { useTranslation, Trans } from "react-i18next";

import authorAvatarUrl from "./author-avatar.png";
import githubLogoUrl from "./github.png";

import Card from "../common/Card";

import "./index.css";

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
    name: "vite",
    url: "https://vitejs.dev",
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

export default function AboutPage() {
  const { t } = useTranslation();

  return (
    <div className="px-2 mb-4">
      <Card className="container mt-4 py-3">
        <h4 id="author-info">{t("about.author.title")}</h4>
        <div>
          <div className="d-block">
            <img
              src={authorAvatarUrl}
              className="cru-avatar large cru-round cru-float-left"
            />
            <p>
              <small>{t("about.author.name")}</small>
              <span className="cru-color-primary">crupest</span>
            </p>
            <p>
              <small>{t("about.author.introduction")}</small>
              {t("about.author.introductionContent")}
            </p>
          </div>
          <p>
            <small>{t("about.author.links")}</small>
            <a
              href="https://github.com/crupest"
              target="_blank"
              rel="noopener noreferrer"
            >
              <img src={githubLogoUrl} className="about-link-icon" />
            </a>
          </p>
        </div>
      </Card>
      <Card className="container mt-4 py-3">
        <h4>{t("about.site.title")}</h4>
        <p>
          <Trans i18nKey="about.site.content">
            0<span className="cru-color-primary">1</span>2<b>3</b>4
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
      </Card>
      <Card className="container mt-4 py-3">
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
      </Card>
    </div>
  );
}
