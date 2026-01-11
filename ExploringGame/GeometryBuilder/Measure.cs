namespace ExploringGame.GeometryBuilder;

public static class Measure
{
    private const float InchesToUnits = 0.04f;

    public static float Inches(float inches) =>  inches * InchesToUnits;

    public static float Feet(float feet) => feet * 12f * InchesToUnits;
}
