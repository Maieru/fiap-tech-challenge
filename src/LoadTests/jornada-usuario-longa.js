import jornadaUsuario, { options as baseOptions } from './jornada-usuario.js';

const START_VUS = Number(__ENV.START_VUS || 2);
const MAX_VUS = Number(__ENV.MAX_VUS || 10);

if (!Number.isInteger(START_VUS) || !Number.isInteger(MAX_VUS)
    || START_VUS < 1 || MAX_VUS < START_VUS) {
  throw new Error('START_VUS e MAX_VUS devem ser inteiros positivos, com START_VUS <= MAX_VUS.');
}

export const options = {
  ...baseOptions,
  scenarios: {
    jornada_usuario_longa: {
      executor: 'ramping-vus',
      startVUs: START_VUS,
      gracefulRampDown: '30s',
      stages: [
        { duration: __ENV.STAGE_DURATION || '5m', target: MAX_VUS },
        { duration: __ENV.HOLD_DURATION || '50m', target: MAX_VUS },
        { duration: __ENV.RAMP_DOWN_DURATION || '5m', target: 0 },
      ],
    },
  },
};

export default jornadaUsuario;
