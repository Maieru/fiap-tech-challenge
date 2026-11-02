export function redactPrivateQuery(message: string) {
  return message.replace(/([?&]token=)[^&\s"'<>]+/gi, "$1[redacted]");
}
