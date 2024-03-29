import { useState } from "react";

export { default as Dialog } from "./Dialog";
export { default as FullPageDialog } from "./FullPageDialog";
export { default as OperationDialog } from "./OperationDialog";
export { default as ConfirmDialog } from "./ConfirmDialog";
export { default as DialogContainer } from "./DialogContainer";

type DialogMap<D extends string, V> = {
  [K in D]: V;
};

type DialogKeyMap<D extends string> = DialogMap<D, number>;

type DialogPropsMap<D extends string> = DialogMap<
  D,
  { key: number | string; open: boolean; onClose: () => void }
>;

export function useDialog<D extends string>(
  dialogs: D[],
  options?: {
    initDialog?: D | null;
    onClose?: {
      [K in D]?: () => void;
    };
  },
): {
  dialog: D | null;
  switchDialog: (newDialog: D | null) => void;
  dialogPropsMap: DialogPropsMap<D>;
  createDialogSwitch: (newDialog: D | null) => () => void;
} {
  const [dialog, setDialog] = useState<D | null>(options?.initDialog ?? null);

  const [dialogKeys, setDialogKeys] = useState<DialogKeyMap<D>>(
    () => Object.fromEntries(dialogs.map((d) => [d, 0])) as DialogKeyMap<D>,
  );

  const switchDialog = (newDialog: D | null) => {
    if (dialog !== null) {
      setDialogKeys({ ...dialogKeys, [dialog]: dialogKeys[dialog] + 1 });
    }
    setDialog(newDialog);
  };

  return {
    dialog,
    switchDialog,
    dialogPropsMap: Object.fromEntries(
      dialogs.map((d) => [
        d,
        {
          // TODO: Fix the animation problem, maybe we have to use a better way to re-enable this.
          //key: `${d}-${dialogKeys[d]}`,
          open: dialog === d,
          onClose: () => {
            switchDialog(null);
            options?.onClose?.[d]?.();
          },
        },
      ]),
    ) as DialogPropsMap<D>,
    createDialogSwitch: (newDialog: D | null) => () => switchDialog(newDialog),
  };
}
