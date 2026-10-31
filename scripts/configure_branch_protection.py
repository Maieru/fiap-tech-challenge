"""Apply the reviewed policy to this repository, preserving stronger existing protections.

Uses GH_TOKEN or noninteractive Git Credential Manager; credentials are never printed.
Without --apply, only reports the current protection status.
"""
import argparse
import json
import os
from pathlib import Path
import subprocess
import urllib.error
import urllib.request

ROOT = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--apply', action='store_true')
args = parser.parse_args()
remote = subprocess.check_output(['git', 'remote', 'get-url', 'origin'], cwd=ROOT, text=True).strip()
if remote.lower().removesuffix('.git') != 'https://github.com/maieru/fiap-tech-challenge':
    raise SystemExit('Unexpected origin. No remote settings were changed.')
access_token = os.environ.get('GH_TOKEN')
if not access_token:
    environment = dict(os.environ, GIT_TERMINAL_PROMPT='0', GCM_INTERACTIVE='never')
    credential = subprocess.run(['git', 'credential', 'fill'], input='protocol=https\nhost=github.com\n\n',
                                capture_output=True, text=True, env=environment, cwd=ROOT, timeout=20)
    fields = dict(line.split('=', 1) for line in credential.stdout.splitlines() if '=' in line)
    access_token = fields.get('password')
if not access_token:
    raise SystemExit('Noninteractive GitHub authentication unavailable. Configure the policy through GitHub Settings or GH_TOKEN.')

base = 'https://api.github.com/repos/Maieru/fiap-tech-challenge/branches/main/protection'


def call(method, suffix='', body=None):
    request = urllib.request.Request(base + suffix, data=None if body is None else json.dumps(body).encode(), method=method,
        headers={'Authorization': 'Bearer ' + access_token, 'Accept': 'application/vnd.github+json',
                 'X-GitHub-Api-Version': '2022-11-28', 'Content-Type': 'application/json'})
    try:
        with urllib.request.urlopen(request, timeout=20) as response:
            data = response.read()
            return json.loads(data) if data else {}
    except urllib.error.HTTPError as error:
        if error.code == 404 and method == 'GET':
            return None
        raise SystemExit(f'GitHub returned HTTP {error.code}; operation did not complete. Credentials were not logged.') from None


current = call('GET')
if args.apply:
    policy = json.loads((ROOT / '.github/branch-protection.json').read_text(encoding='utf-8'))
    if current is None:
        call('PUT', body=policy)
    else:
        existing = current.get('required_status_checks') or {}
        checks = existing.get('checks') or [{'context': context, 'app_id': -1} for context in existing.get('contexts', [])]
        if not any(check['context'] == 'ci-required' for check in checks):
            checks.append({'context': 'ci-required', 'app_id': -1})
        def actors(value):
            return {kind: [entry['login' if kind == 'users' else 'slug'] for entry in (value or {}).get(kind, [])]
                    for kind in ['users', 'teams', 'apps']}

        reviews = current.get('required_pull_request_reviews')
        if reviews:
            review_policy = {key: reviews[key] for key in [
                'dismiss_stale_reviews', 'require_code_owner_reviews', 'required_approving_review_count', 'require_last_push_approval'
            ] if key in reviews}
            for key in ['dismissal_restrictions', 'bypass_pull_request_allowances']:
                if reviews.get(key) is not None:
                    review_policy[key] = actors(reviews[key])
            policy['required_pull_request_reviews'] = review_policy
        if current.get('restrictions') is not None:
            policy['restrictions'] = actors(current['restrictions'])
        for flag in ['required_linear_history', 'block_creations', 'required_conversation_resolution', 'lock_branch', 'allow_fork_syncing']:
            if flag in current:
                policy[flag] = current[flag].get('enabled', False)
        policy['required_status_checks'] = {'strict': True, 'checks': checks}
        # A disabled status-check subresource cannot be enabled through PATCH (404).
        # PUT the merged parent policy, preserving existing review/actor/branch restrictions.
        call('PUT', body=policy)
        if (current.get('required_signatures') or {}).get('enabled'):
            call('POST', '/required_signatures')
    current = call('GET')
print(json.dumps({
    'protected': current is not None,
    'requires_pull_request': bool((current or {}).get('required_pull_request_reviews')),
    'required_contexts': sorted(set(((current or {}).get('required_status_checks') or {}).get('contexts', [])
        + [check['context'] for check in ((current or {}).get('required_status_checks') or {}).get('checks', [])])),
    'enforce_admins': ((current or {}).get('enforce_admins') or {}).get('enabled', False)
}, indent=2))
