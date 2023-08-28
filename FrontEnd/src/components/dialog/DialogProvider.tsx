import { useState, useContext, createContext, ReactNode } from "react";

import { UiLogicError } from "../common";

type DialogMap<D extends string> = {
  [K in D]: ReactNode;
};

interface DialogController<D extends string> {
  currentDialog: D | null;
  currentDialogReactNode: ReactNode;
  canSwitchDialog: boolean;
  switchDialog: (newDialog: D | null) => void;
  setCanSwitchDialog: (enable: boolean) => void;
  closeDialog: () => void;
  forceSwitchDialog: (newDialog: D | null) => void;
  forceCloseDialog: () => void;
}

export function useDialog<D extends string>(
  dialogs: DialogMap<D>,
  options?: {
    initDialog?: D | null;
    onClose?: {
      [K in D]?: () => void;
    };
  },
): {
  controller: DialogController<D>;
  switchDialog: (newDialog: D | null) => void;
  forceSwitchDialog: (newDialog: D | null) => void;
  createDialogSwitch: (newDialog: D | null) => () => void;
} {
  const [canSwitchDialog, setCanSwitchDialog] = useState<boolean>(true);
  const [dialog, setDialog] = useState<D | null>(options?.initDialog ?? null);

  const forceSwitchDialog = (newDialog: D | null) => {
    if (dialog != null) {
      options?.onClose?.[dialog]?.();
    }
    setDialog(newDialog);
    setCanSwitchDialog(true);
  };

  const switchDialog = (newDialog: D | null) => {
    if (canSwitchDialog) {
      forceSwitchDialog(newDialog);
    }
  };

  const controller: DialogController<D> = {
    currentDialog: dialog,
    currentDialogReactNode: dialog == null ? null : dialogs[dialog],
    canSwitchDialog,
    switchDialog,
    setCanSwitchDialog,
    closeDialog: () => switchDialog(null),
    forceSwitchDialog,
    forceCloseDialog: () => forceSwitchDialog(null),
  };

  return {
    controller,
    switchDialog,
    forceSwitchDialog,
    createDialogSwitch: (newDialog: D | null) => () => switchDialog(newDialog),
  };
}

const DialogControllerContext = createContext<DialogController<string> | null>(
  null,
);

export function useDialogController(): DialogController<string> {
  const controller = useContext(DialogControllerContext);
  if (controller == null) throw new UiLogicError("not in dialog provider");
  return controller;
}

export function useCloseDialog(): () => void {
  const controller = useDialogController();
  return controller.closeDialog;
}

export function DialogProvider<D extends string>({
  controller,
}: {
  controller: DialogController<D>;
}) {
  return (
    <DialogControllerContext.Provider value={controller as never}>
      {controller.currentDialogReactNode}
    </DialogControllerContext.Provider>
  );
}
