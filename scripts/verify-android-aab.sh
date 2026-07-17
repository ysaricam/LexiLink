#!/usr/bin/env bash
set -euo pipefail

aab_path="${1:-frontend/build/app/outputs/bundle/release/app-release.aab}"
manifest_path="frontend/build/app/intermediates/merged_manifests/release/processReleaseManifest/AndroidManifest.xml"

if [[ ! -f "$aab_path" ]]; then
  echo "AAB not found: $aab_path" >&2
  exit 1
fi

if [[ ! -f "$manifest_path" ]]; then
  manifest_path="$(find frontend/build/app/intermediates/merged_manifests/release -name AndroidManifest.xml -print -quit)"
fi

grep -q 'package="com.wordlope.app"' "$manifest_path"
grep -q 'android:minSdkVersion="24"' "$manifest_path"
grep -q 'android:targetSdkVersion="36"' "$manifest_path"
grep -q 'android.permission.INTERNET' "$manifest_path"

pubspec_version="$(sed -n 's/^version: //p' frontend/pubspec.yaml)"
expected_version_name="${pubspec_version%%+*}"
expected_version_code="${pubspec_version##*+}"
grep -q "android:versionName=\"$expected_version_name\"" "$manifest_path"
grep -q "android:versionCode=\"$expected_version_code\"" "$manifest_path"

android_sdk_root="${ANDROID_SDK_ROOT:-${ANDROID_HOME:-}}"
if [[ -z "$android_sdk_root" ]]; then
  echo "ANDROID_SDK_ROOT or ANDROID_HOME must be set for the 16 KB ELF check." >&2
  exit 1
fi

objdump="$(find "$android_sdk_root/ndk" -path '*/toolchains/llvm/prebuilt/*/bin/llvm-objdump' -print | sort -V | tail -1)"
if [[ ! -x "$objdump" ]]; then
  echo "llvm-objdump was not found in the Android NDK." >&2
  exit 1
fi

tmp_dir="$(mktemp -d)"
trap 'rm -rf "$tmp_dir"' EXIT
unzip -q "$aab_path" 'base/lib/*/*.so' -d "$tmp_dir"

while IFS= read -r library; do
  while IFS= read -r load_line; do
    alignment="${load_line##*align 2**}"
    if [[ "$alignment" =~ ^[0-9]+$ ]] && (( alignment < 14 )); then
      echo "16 KB incompatible LOAD alignment in $library: 2**$alignment" >&2
      exit 1
    fi
  done < <("$objdump" -p "$library" | grep '^[[:space:]]*LOAD ')
done < <(find "$tmp_dir" -name '*.so' -type f | sort)

echo "Verified package, version $expected_version_name+$expected_version_code, minSdk 24, targetSdk 36, INTERNET permission, and 16 KB ELF alignment."
