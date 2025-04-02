#!/bin/bash
cd /path/to/dev-diary
git add .
git commit -m "자동 업데이트: $(date '+%Y-%m-%d') 개발 일지"
git push origin main