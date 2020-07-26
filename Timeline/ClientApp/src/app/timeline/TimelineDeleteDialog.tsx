import React from 'react';
import { useHistory } from 'react-router';
import { Trans } from 'react-i18next';

import OperationDialog from '../common/OperationDialog';
import { timelineService } from '../data/timeline';

interface TimelineDeleteDialog {
  open: boolean;
  name: string;
  close: () => void;
}

const TimelineDeleteDialog: React.FC<TimelineDeleteDialog> = (props) => {
  const history = useHistory();

  const { name } = props;

  return (
    <OperationDialog
      open={props.open}
      close={props.close}
      title="timeline.deleteDialog.title"
      titleColor="danger"
      inputPrompt={() => {
        return (
          <Trans i18nKey="timeline.deleteDialog.inputPrompt">
            0<code className="mx-2">{{ name }}</code>2
          </Trans>
        );
      }}
      inputScheme={[
        {
          type: 'text',
          validator: (value) => {
            if (value !== name) {
              return 'timeline.deleteDialog.notMatch';
            } else {
              return null;
            }
          },
        },
      ]}
      onProcess={() => {
        return timelineService.deleteTimeline(name).toPromise();
      }}
      onSuccessAndClose={() => {
        history.replace('/');
      }}
    />
  );
};

export default TimelineDeleteDialog;
