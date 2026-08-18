# Adaptación de Extreme Programming para una persona

Este proyecto fue desarrollado por una sola persona. Por ello se aplican las
prácticas técnicas y de planificación de XP que pueden verificarse
individualmente y se identifican sin ambigüedad las que requieren un equipo o
un cliente independiente.

## Criterio de aplicación

| Práctica XP | Aplicación en el proyecto | Evidencia |
|---|---|---|
| Planning Game | Priorización, estimación y selección de historias por iteración | Historias, Milestones, plan y bitácora |
| Pequeñas liberaciones | Incremento demostrable al terminar cada iteración | Tags, Releases y guiones de demostración |
| TDD | Pruebas antes o junto a la implementación y ciclos rojo-verde-refactor identificables | Commits y suites automatizadas |
| Integración continua | Integración frecuente con verificación automática | Pull Requests y GitHub Actions |
| Diseño simple | Implementación mínima, separación de responsabilidades y eliminación de duplicación | Arquitectura y refactorizaciones registradas |
| Refactorización | Mejoras estructurales protegidas por pruebas | Commits `refactor` y bitácora |
| Estándares de código | Convenciones, analizadores y formato automático | `.editorconfig`, `Directory.Build.props` y CI |
| Pruebas de aceptación | Criterios verificables y guiones de demostración | Historias y `docs/releases/` |
| Ritmo sostenible | Se trabajó por iteraciones; no se conservaron horas fiables para demostrar carga diaria | Evidencia insuficiente; no se reclama cumplimiento pleno |
| Programación en pareja | Requiere dos desarrolladores humanos | No aplicable al proyecto individual |
| Propiedad colectiva | Requiere varias personas con responsabilidad compartida | No aplicable al proyecto individual |
| Cliente presente | No existió un cliente independiente permanente | La priorización y aceptación registradas son autoevaluaciones del responsable |

## Uso de inteligencia artificial

La IA se utilizó como herramienta de asistencia para analizar, proponer y
revisar trabajo. El responsable humano tomó las decisiones, ejecutó las
verificaciones y conserva la autoría. Esta colaboración no se contabiliza como
*pair programming* ni como propiedad colectiva. El detalle se encuentra en
[uso-ia.md](uso-ia.md).

## Interpretación de la retroalimentación

Cuando la bitácora histórica usa la palabra "cliente", se refiere al rol de
producto asumido por Robert Granados durante la autoevaluación, no a una
persona externa. Las listas de aceptación demuestran verificación funcional,
pero no sustituyen evidencia de validación independiente.

## Evaluación honesta

El proyecto demuestra con solidez las prácticas técnicas de XP y adapta la
planificación iterativa al trabajo individual. No afirma cumplimiento estricto
de las prácticas sociales que, por definición, necesitan más participantes.
