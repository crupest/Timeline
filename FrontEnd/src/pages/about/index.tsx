import "./index.css";

import { useC } from "@/common";
import Page from "@/views/common/Page";

interface Credit {
  name: string;
  url: string;
}

type Credits = Credit[];

const frontendCredits: Credits = [
  {
    name: "react.js",
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
    name: "parcel.js",
    url: "https://parceljs.org",
  },
  {
    name: "eslint",
    url: "https://eslint.org",
  },
  {
    name: "prettier",
    url: "https://prettier.io",
  },
];

const backendCredits: Credits = [
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
  const c = useC();

  return (
    <Page className="about-page">
      <h2>{c("about.credits.title")}</h2>
      <p>{c("about.credits.content")}</p>
      <h3>{c("about.credits.frontend")}</h3>
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
      <h3>{c("about.credits.backend")}</h3>
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
    </Page>
  );
}
