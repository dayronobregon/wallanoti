# 📊 Wallanoti NER (Named Entity Recognition)

## 📋 Descripción

La herramienta **NER** (Named Entity Recognition) es un sistema de extracción de entidades desarrollado en **Python con spaCy** que analiza el texto escrito por los usuarios para identificar automáticamente:

- **PRODUCTO**: El artículo que el usuario busca (ej. "iPhone 15", "televisor 50 pulgadas", "PS5")
- **UBICACIÓN**: La zona/geografía de interés (ej. "Murcia", "Madrid", "Barcelona")
- **PRECIO MÍNIMO**: Límite inferior de precio (ej. "más de 300€")
- **PRECIO MÁXIMO**: Límite superior de precio (ej. "menos de 800€")

### Ejemplo de uso

**Input:**
```
"quiero comprar un televisor de 40 pulgadas por menos de 500 euros en murcia"
```

**Output:**
```json
{
  "text": "quiero comprar un televisor de 40 pulgadas por menos de 500 euros en murcia",
  "product": "televisor de 40 pulgadas",
  "location": "murcia",
  "min_price": null,
  "max_price": "500"
}
```

---

## 🏗️ Arquitectura del sistema

### Diseño Clean/Hexagonal

El proyecto sigue los principios de **Clean Architecture** y **Hexagonal Architecture**:

```
┌─────────────────────────────────────────────────────┐
│                 User Interface (API)                │
│      FastAPI endpoints /extract, /health           │
└─────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────┐
│                   Application Layer                 │
│              ExtractEntities Use Case               │
└─────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────┐
│                    Domain Layer                     │
│         NerExtractor (interface) + Domain Models    │
└─────────────────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────┐
│                Infrastructure Layer                 │
│      SpacyNerModel (implementation) + spaCy pipes   │
└─────────────────────────────────────────────────────┘
```

**Componentes clave:**

| Capa | Archivo | Responsabilidad |
|------|---------|-----------------|
| **API** | `app/api/v1/endpoints/extraction.py` | Endpoint `/v1/ner/extract` con validación |
| **Application** | `app/application/entity_extractor/extract_entities.py` | Orquestación del caso de uso |
| **Domain** | `app/domain/entity_extractor.py` | Interface `NerExtractor` |
| **Infrastructure** | `app/infrastructure/nlp/spacy_ner_model.py` | Implementación con spaCy |

---

## 🧪 Dataset y entrenamiento

### Generación de datos sintéticos

El dataset se genera automáticamente con **anotación precisa** usando plantillas realistas:

- **2500 ejemplos** configurables
- **Distribución ponderada** que refleja patrones reales de búsqueda:
  - 30% → Precio implícito max ("un iPhone de 500€")
  - 20% → Precio máximo explícito ("no más de 800€")
  - 15% → Rango de precio ("entre 200 y 400€")
  - 10% → Precio mínimo ("más de 150€")
  - 25% → Sin precio (solo producto/ubicación)

**Vocabulario cubierto:**
- **40+ productos**: electrónica (iPhone, PS5, tablets), hogar (sofá, nevera), moda, deporte
- **20+ ubicaciones**: ciudades españolas (Murcia, Madrid, Barcelona, etc.)

**Estructura del dataset:**
```python
[
  ("quiero un iphone 15 de 600 euros en murcia", {
    "entities": [
      (8, 20, "PRODUCT"),
      (39, 45, "MAX_PRICE"),
      (50, 55, "LOCATION")
    ]
  }),
  # ...
]
```

### Entrenamiento

El modelo se entrena con **two approaches**:

#### 1. **Entorno puro (NER desde cero)**
```bash
cd ner
python training/train.py
```
- Crea un modelo en blanco `es` solo con NER
- 30 iteraciones con dropout 0.2
- Guarda en `models/ner_v1/`

#### 2. **Fine-tuning (NER + es_core_news_md)** ⭐ recomendado
```bash
cd ner
python training/train_core.py
```
- Carga `es_core_news_md` como base (vectors + tok2vec preentrenados)
- Fine-tunea solo el pipe `ner` (evita catastrophic forgetting)
- Guarda en `models/ner_core_v1/`

**Métricas de evaluación:**
- **Precisión (P)**: % de entidades extraídas que son correctas
- **Recall (R)**: % de entidades reales que se detectaron
- **F1-Score**: Media armónica de P y R

---

## 🚀 Uso de la API

### Iniciar el servidor

```bash
cd ner
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

### Endpoints disponibles

#### `/health` (GET)
Verifica que el servicio esté operativo.

**Response:**
```json
{"status": "ok"}
```

#### `/v1/ner/extract` (GET)

**Query Parameters:**
- `text` (required): Texto a analizar

**Ejemplo de uso:**
```bash
curl "http://localhost:8000/v1/ner/extract?text=quiero%20comprar%20un%20televisor%20de%2040%20pulgadas%20por%20menos%20de%20500%20euros%20en%20murcia"
```

**Response:**
```json
{
  "text": "quiero comprar un televisor de 40 pulgadas por menos de 500 euros en murcia",
  "product": "televisor de 40 pulgadas",
  "location": "murcia",
  "min_price": null,
  "max_price": "500"
}
```

**Errores:**
- `422 Unprocessable Entity`: Texto vacío o no procesable

---

## 🧱 Estructura del proyecto

```
ner/
├── app/
│   ├── api/
│   │   ├── v1/
│   │   │   ├── __init__.py
│   │   │   ├── router.py          # Ruta /v1
│   │   │   └── endpoints/
│   │   │       └── extraction.py  # Endpoint /ner/extract
│   │   └── dependencies.py        # Dependencias FastAPI
│   ├── application/
│   │   └── entity_extractor/
│   │       └── extract_entities.py  # Use case
│   ├── domain/
│   │   ├── entity_extractor.py    # Interface NER
│   │   ├── entity_model.py        # Value objects
│   │   └── extraction_model.py    # Resultado
│   └── infrastructure/
│       └── nlp/
│           ├── __init__.py
│           └── spacy_ner_model.py # Implementación spaCy
├── training/
│   ├── data/
│   │   ├── __init__.py
│   │   └── generate_data.py       # Generación dataset sintético
│   ├── train.py                   # Entrenamiento desde cero
│   └── train_core.py              # Fine-tuning con es_core_news_md
├── models/                        # MODELOS NO VERSIONADOS
│   ├── ner_v1/                    # Modelo entrenado desde cero
│   └── ner_core_v1/               # Modelo fine-tuned
├── .env                           # Variables de entorno
├── main.py                        # App FastAPI principal
├── requirements.txt
└── test_main.http                 # Tests HTTP (VS Code REST Client)
```

---

## 🛠️ Instalación y desarrollo local

### 1. Crear entorno virtual

```bash
cd ner
python3 -m venv .venv
source .venv/bin/activate  # Linux/macOS
# .venv\Scripts\activate   # Windows
```

### 2. Instalar dependencias

```bash
pip install -r requirements.txt
python -m spacy download es_core_news_md  # Modelo base para fine-tuning
```

### 3. Entrenar modelo (opcional)

```bash
cd training
python train_core.py  # O: python train.py
```

### 4. Ejecutar API

```bash
cd ..
uvicorn main:app --reload --host 0.0.0.0 --port 8000
```

---

## 📝 Consideraciones técnicas

### ¿Por qué **no versionar modelos**?

- **Tamaño**: Los modelos spaCy `. spaCy` rondan **100-500MB**
- **Entrenamiento en servidor**: Se construyen en el entorno de despliegue
- **Especificidad**: Cada modelo está entrenado con datos específicos del servidor

### ¿Cómo usar el modelo en producción?

1. **Entrenar en el servidor** (como se detalla en este documento)
2. **Descargar el modelo** generado (`models/ner_core_v1/`)
3. **Cargar en tiempo de ejecución**:

```python
# app/infrastructure/nlp/spacy_ner_model.py
nlp = spacy.load("models/ner_core_v1")
```

### Optimizaciones

- **Fine-tuning en vez de entrenar desde cero**: Evita catastrophic forgetting
- **Selección de pipes**: `nlp.select_pipes(enable=["ner"])` congela tok2vec
- **Batch compounding**: Tamaño de batch que crece de 4 a 32
- **Dropout 0.2**: Regularización para evitar overfitting

---

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Si quieres mejorar el modelo:

1. **Propón mejoras en el dataset** (nuevas plantillas, vocabulario)
2. **Añade entidades adicionales** (categoría, marca, estado)
3. **Optimiza hiperparámetros** (iteraciones, dropout, batch size)
4. **Prueba otros modelos base** (es_core_news_lg, modelos multilingües)

---

## 📚 Recursos

- **spaCy Documentation**: https://spacy.io/
- **spaCy Training**: https://spacy.io/usage/training
- **Clean Architecture**: https://blog.clearcheveloper.com/architectura-limpi
- **Named Entity Recognition**: https://en.wikipedia.org/wiki/Named_entity_recognition

---
