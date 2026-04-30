from fastapi import FastAPI

from app.api.v1.router import router as v1_router

app = FastAPI(
    title="Wallanoti NER API",
    description="Extracción de entidades (producto, ubicación, precios) con spaCy.",
    version="1.0.0",
)

app.include_router(v1_router)


@app.get("/health", tags=["Health"])
def health_check() -> dict:
    return {"status": "ok"}
