"""Merge Cobertura line hits by source location, then enforce a component gate."""
import argparse
import json
from pathlib import Path
import xml.etree.ElementTree as ET


def measure(directory: Path, component: str):
    lines = {}
    prefixes = {
        "legacy": "FIAP.TechChallenge.Fase1.",
        "chassis": "Maieru.FiapTechChallenge.Chassis",
        "os": "FIAP.TechChallenge.OS.",
    }
    reports = list(directory.rglob("coverage.cobertura.xml"))
    for report in reports:
        for package in ET.parse(report).getroot().findall("./packages/package"):
            name = package.get("name", "")
            if not name.startswith(prefixes[component]) or ".Tests" in name or name.endswith(".Sample"):
                continue
            for cls in package.findall("./classes/class"):
                filename = cls.get("filename", "").replace("\\", "/")
                if "/Migrations/" in filename or filename.endswith((".g.cs", ".generated.cs")):
                    continue
                # Reports from different test projects use different source roots.
                filename = filename[filename.find("src/"):] if "src/" in filename else filename
                for line in cls.findall("./lines/line"):
                    key = (name, filename, int(line.attrib["number"]))
                    lines[key] = lines.get(key, False) or int(line.attrib["hits"]) > 0
    if not lines:
        raise ValueError(f"No instrumented source lines found for {component} in {directory}")
    covered = sum(lines.values())
    return {"component": component, "covered": covered, "total": len(lines),
            "percent": round(100 * covered / len(lines), 2), "reports": len(reports)}


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("component", choices=["legacy", "chassis", "os"])
    parser.add_argument("directory", type=Path)
    parser.add_argument("--minimum", type=float, default=80)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    result = measure(args.directory, args.component)
    print(json.dumps(result, indent=2))
    if args.output:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    raise SystemExit(0 if result["covered"] * 100 >= args.minimum * result["total"] else 1)
