import {test} from 'node:test';
import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {createRequire} from 'node:module';

const {parse} = createRequire(new URL('../delivery/package.json', import.meta.url))('yaml');

const {jobs} = parse(readFileSync(new URL('../../.github/workflows/manage-platform.yml', import.meta.url), 'utf8'));

function dependsOn(job, dependency, visited = new Set()) {
  if (visited.has(job)) return false;
  visited.add(job);
  return (jobs[job].needs ?? []).some(need => need === dependency || dependsOn(need, dependency, visited));
}

test('destroy encerra apps, apaga dados e so depois remove controllers e cluster', () => {
  const order = ['destroy_edge', 'cleanup', 'destroy_messaging', 'destroy_database',
    'destroy_configs', 'destroy_addons', 'destroy_core', 'verify_destroy'];
  for (let index = 1; index < order.length; index++) {
    assert.ok(dependsOn(order[index], order[index - 1]), `${order[index]} deve aguardar ${order[index - 1]}`);
    assert.doesNotMatch(jobs[order[index]].if ?? '', /always\(\)|failure\(\)/);
  }
});

test('RDS e MongoDB fazem parte de todo destroy, inclusive agendamento', () => {
  assert.equal(jobs.resolve.outputs.operation, "${{ inputs.operation || 'destroy' }}");
  for (const [job, repository, stage, role] of [
    ['destroy_messaging', 'infra', 'messaging', 'INFRA_ACTION_ROLE'],
    ['destroy_database', 'db', 'database', 'DATABASE_ACTION_ROLE']
  ]) {
    const config = jobs[job];
    assert.equal(config.if, "needs.resolve.outputs.operation == 'destroy'");
    assert.equal(config.uses, `Maieru/fiap-tech-challenge-${repository}/.github/workflows/terraform-stage.yml@main`);
    assert.equal(config.with.stage, stage);
    assert.equal(config.with.operation, 'destroy');
    assert.equal(config.with.role_arn, '${{ vars.' + role + ' }}');
  }
});
