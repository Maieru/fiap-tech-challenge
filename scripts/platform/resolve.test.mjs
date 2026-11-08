import assert from 'node:assert/strict';
import {test} from 'node:test';
import {spawnSync} from 'node:child_process';
import {mkdtempSync, readFileSync, rmSync} from 'node:fs';
import {tmpdir} from 'node:os';
import {join} from 'node:path';

function resolve(operation, secrets = {}) {
  const directory = mkdtempSync(join(tmpdir(), 'fiap-resolve-'));
  try {
    const environment = {...process.env, GITHUB_REF: 'refs/heads/main', PLATFORM_ENABLED: 'true', OPERATION: operation,
      GITHUB_SHA: 'a'.repeat(40), GH_TOKEN: 'test-token', GITHUB_OUTPUT: join(directory, 'output'),
      GITHUB_STEP_SUMMARY: join(directory, 'summary'), INFRA_ACTION_ROLE: 'infra', DATABASE_ACTION_ROLE: 'db',
      HAS_NEW_RELIC_LICENSE: 'true', HAS_MERCADO_PAGO_ACCESS_TOKEN: 'false', HAS_MERCADO_PAGO_WEBHOOK_SECRET: 'false',
      JWT_ISSUER: 'issuer', JWT_AUDIENCE: 'audience'};
    for (const service of ['OS', 'EXECUTION', 'BILLING'])
      for (const action of ['PUBLISH', 'DEPLOY']) environment['AWS_' + service + '_' + action + '_ROLE_ARN'] = 'test-role';
    Object.assign(environment, secrets);
    const script = `
      globalThis.fetch = async url => ({ok:true,json:async()=>url.includes('/actions/')
        ? {workflow_runs:[{head_sha:'${'a'.repeat(40)}',head_branch:'main',event:'push',run_number:1,status:'completed',conclusion:'success'}]}
        : {object:{sha:'${'a'.repeat(40)}'}}});
      await import(${JSON.stringify(new URL('./resolve.mjs', import.meta.url).href)});
    `;
    const result = spawnSync(process.execPath, ['--input-type=module', '--eval', script], {env: environment, encoding: 'utf8'});
    return {...result, output: result.status === 0 ? readFileSync(environment.GITHUB_OUTPUT, 'utf8') : null};
  } finally { rmSync(directory, {recursive: true, force: true}); }
}

test('apply exige ambos os secrets Mercado Pago antes de resolver fontes', () => {
  const missingToken = resolve('apply');
  assert.notEqual(missingToken.status, 0);
  assert.match(missingToken.stderr, /Configure o secret MERCADO_PAGO_ACCESS_TOKEN/);
  const missingSignature = resolve('apply', {HAS_MERCADO_PAGO_ACCESS_TOKEN: 'true'});
  assert.notEqual(missingSignature.status, 0);
  assert.match(missingSignature.stderr, /Configure o secret MERCADO_PAGO_WEBHOOK_SECRET/);
});
test('apply com secrets presentes seleciona os commits aprovados', () => {
  const result = resolve('apply', {HAS_MERCADO_PAGO_ACCESS_TOKEN: 'true', HAS_MERCADO_PAGO_WEBHOOK_SECRET: 'true'});
  assert.equal(result.status, 0, result.stderr);
  assert.match(result.output, /billing_sha=a{40}/);
});
test('destroy não depende de secrets de pagamento', () => {
  const result = resolve('destroy');
  assert.equal(result.status, 0, result.stderr);
  assert.match(result.output, /infra_sha=a{40}/);
});
