#!/usr/bin/env python3
"""Verificador de umbrales de cobertura (HU-47).

Uso:
    python3 tools/check-coverage.py <directorio|archivo> [<directorio|archivo>...]

Busca los archivos coverage.cobertura.xml generados por coverlet
(colección "XPlat Code Coverage") dentro de los directorios indicados,
los combina y valida los umbrales de cobertura de líneas:

    - Licitaciones.Domain         >= 80 %
    - Licitaciones.Application    >= 80 %
    - Núcleo (Domain + Application
      + Infrastructure)           >= 70 %

Se excluye código generado: rutas bajo obj/ y bajo Persistence/Migrations/.

Salida: código 0 si se cumplen todos los umbrales; 1 si alguno no se cumple.
"""

import argparse
import glob
import os
import sys
import xml.etree.ElementTree as ET

PROYECTOS_NUCLEO = {
    "Licitaciones.Domain",
    "Licitaciones.Application",
    "Licitaciones.Infrastructure",
}

UMBRALES = {
    "Licitaciones.Domain": 80.0,
    "Licitaciones.Application": 80.0,
    "Núcleo (Domain + Application + Infrastructure)": 70.0,
}


def clave_canonica(ruta):
    """Normaliza la ruta para unir archivos reportados con rutas relativas o absolutas."""
    ruta = ruta.replace("\\", "/")
    partes = ruta.split("/")
    for i, parte in enumerate(partes):
        if parte in PROYECTOS_NUCLEO:
            return "/".join(partes[i:])
    return ruta


def es_codigo_generado(clave):
    return "/obj/" in clave or clave.startswith("obj/") or "/Persistence/Migrations/" in clave


def archivos_cobertura(entradas):
    encontrados = []
    for entrada in entradas:
        if os.path.isdir(entrada):
            encontrados.extend(glob.glob(os.path.join(entrada, "**", "coverage.cobertura.xml"), recursive=True))
        else:
            encontrados.extend(glob.glob(entrada))
    return sorted(set(encontrados))


def combinar(archivos):
    """Combina hits de línea: por archivo y número de línea se toma el máximo de hits."""
    lineas = {}
    for archivo in archivos:
        arbol = ET.parse(archivo)
        for clase in arbol.iter("class"):
            nombre = clase.get("filename")
            if not nombre:
                continue
            clave = clave_canonica(nombre)
            for linea in clase.iter("line"):
                numero = int(linea.get("number"))
                hits = int(linea.get("hits"))
                lineas.setdefault(clave, {})[numero] = max(lineas.get(clave, {}).get(numero, 0), hits)
    return lineas


def cobertura_por_proyecto(lineas):
    proyectos = {}
    for clave, hits_por_linea in lineas.items():
        if es_codigo_generado(clave):
            continue
        proyecto = clave.split("/")[0]
        if proyecto not in PROYECTOS_NUCLEO:
            continue
        cubiertas = sum(1 for hits in hits_por_linea.values() if hits > 0)
        datos = proyectos.setdefault(proyecto, {"cubiertas": 0, "totales": 0})
        datos["cubiertas"] += cubiertas
        datos["totales"] += len(hits_por_linea)
    return proyectos


def porcentaje(cubiertas, totales):
    return (100.0 * cubiertas / totales) if totales else 0.0


def principal():
    parser = argparse.ArgumentParser(description="Verifica los umbrales de cobertura de HU-47.")
    parser.add_argument("entradas", nargs="+", help="Directorios o archivos de cobertura.")
    argumentos = parser.parse_args()

    archivos = archivos_cobertura(argumentos.entradas)
    if not archivos:
        print("ERROR: no se encontraron archivos coverage.cobertura.xml en las entradas indicadas.")
        sys.exit(1)

    print(f"Archivos de cobertura combinados: {len(archivos)}")
    for archivo in archivos:
        print(f"  - {archivo}")

    proyectos = cobertura_por_proyecto(combinar(archivos))

    nucleo_cubiertas = sum(d["cubiertas"] for d in proyectos.values())
    nucleo_totales = sum(d["totales"] for d in proyectos.values())
    mediciones = {
        "Licitaciones.Domain": proyectos.get("Licitaciones.Domain", {"cubiertas": 0, "totales": 0}),
        "Licitaciones.Application": proyectos.get("Licitaciones.Application", {"cubiertas": 0, "totales": 0}),
        "Núcleo (Domain + Application + Infrastructure)": {
            "cubiertas": nucleo_cubiertas,
            "totales": nucleo_totales,
        },
    }

    print()
    fallidos = 0
    for nombre, umbral in UMBRALES.items():
        datos = mediciones[nombre]
        valor = porcentaje(datos["cubiertas"], datos["totales"])
        cumple = "OK" if valor >= umbral else "FALLA"
        if valor < umbral:
            fallidos += 1
        print(f"[{cumple}] {nombre}: {valor:6.2f}%  "
              f"(cubiertas {datos['cubiertas']}/{datos['totales']} líneas, umbral {umbral:g}%)")

    print()
    for proyecto, datos in sorted(proyectos.items()):
        print(f"  Detalle {proyecto}: {datos['cubiertas']}/{datos['totales']} líneas "
              f"({porcentaje(datos['cubiertas'], datos['totales']):.2f}%)")

    sys.exit(1 if fallidos else 0)


if __name__ == "__main__":
    principal()
