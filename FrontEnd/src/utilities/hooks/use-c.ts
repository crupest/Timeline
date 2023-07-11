import { useTranslation } from "react-i18next";
import { C, createC } from "../../i18n";

export default function useC(ns?: string): C {
  const { t } = useTranslation(ns);
  return createC(t);
}
