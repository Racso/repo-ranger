# Repo Ranger

The Repo Ranger streamlines the process of managing multiple GitHub repositories by automating cloning and updating files based on specified tags or branches.

It works similarly to package managers like npm or Composer, but it is tool-agnostic and can be used with any type of repository.

## Installation

Download ```ranger.exe``` from the latest release and place it in a directory of your choice.

## Usage

To run the tool, use the following command format:

Minimal:

```shell
ranger
```

```shell
ranger --base-dir <base-directory> --json <json-file> --auth <auth-file> [--lock <lock-file>] [--verbose]
```

### Command-Line Arguments

- `--base-dir`: Specifies the base directory for relative paths. Default: current directory in the CLI.
- `--json`: Path to the JSON file that contains the repository configuration. Default: `<base-dir>/ranger.json`.
- `--auth`: Path to the JSON file that contains GitHub authentication data. Default: `<base-dir>/ranger-auth.json`.
- `--lock`: (Optional) Path to the lock file that manages repository state. Default: `<base-dir>/ranger.lock`.
- `--verbose`, `-v`: (Optional) Increases the logging verbosity for debugging.

## Configuration

### Repository JSON (ranger.json)

```json
{
  "repositories": [
    {
      "url": "https://github.com/user/repo",
      "version": "v1.0.0",
      "destination": "repo-directory"
    },
    {
      "url": "https://github.com/user/repo",
      "version": "v2.*",
      "destination": "repo-directory"
    },
    {
      "url": "https://github.com/user/repo",
      "version": "b:main",
      "destination": "repo-directory"
    }
  ]
}
```

- `url`: The GitHub repository URL.
- `version`: Specifies the branch (`b:branchName`) or tag (`tagName`) to use. Tags can define versions with wildcards (`*`), and Ranger will use the latest version that matches the pattern.
- `destination`: Directory where the repository will be cloned.

### Authentication JSON (ranger-auth.json)

**Note:** Make sure to add this file to your `.gitignore` to avoid exposing sensitive data.

```json
{
  "username": "your-username",
  "token": "your-personal-access-token"
}
```

- `username`: GitHub username.
- `token`: Personal Access Token for authorization.