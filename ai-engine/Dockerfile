FROM mcr.microsoft.com/devcontainers/python:1-3.11

WORKDIR /workspace/ai-engine

COPY requirements.txt ./
RUN pip install --no-cache-dir -r requirements.txt

COPY . .

EXPOSE 8000

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000", "--reload"]
