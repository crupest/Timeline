import { BackendModule } from "i18next";

 const backend: BackendModule = {
  type: "backend",
  init() {
    /* do nothing */
  },
  // eslint-disable-next-line @typescript-eslint/no-misused-promises
  async read(language, namespace) {
    if (namespace === "translation") {
      if (language === "en") {
        return await import("./translations/en/index.json");
      } else if (language === "zh") {
        return await import("./translations/zh/index.json");
      } else {
        throw Error(`Language ${language} is not supported.`);
      }
    } else if (namespace === "admin") {
      if (language === "en") {
        return await import("./translations/en/admin.json");
      } else if (language === "zh") {
        return await import("./translations/zh/admin.json");
      } else {
        throw Error(`Language ${language} is not supported.`);
      }
    } else {
      throw Error(`Namespace ${namespace} is not supported.`);
    }
  },
};

export default backend;

