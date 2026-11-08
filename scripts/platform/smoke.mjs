import {appendFileSync} from 'node:fs';
import {runSmoke} from './smoke-flow.mjs';
const endpoint=process.env.API_ENDPOINT;
if(!/^https:\/\/[a-z0-9]+\.execute-api\.us-east-1\.amazonaws\.com\/?$/.test(endpoint||''))throw new Error('Endpoint AWS invalido.');
const base=endpoint.replace(/\/$/,'');
const message=await runSmoke(base);
if(process.env.GITHUB_STEP_SUMMARY)appendFileSync(process.env.GITHUB_STEP_SUMMARY,message+'\nEndpoint: '+base+'\n');
