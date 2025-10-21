namespace Gamification.Core.GameModels;

public static class ExperienceTableProgressionRule{
    private static int BaseExpThreshold = 1000;
    private static float Exponent = 1.5f;
    public static float[] ExpTable = new float[500]; //

    static ExperienceTableProgressionRule(){
        for (int i = 0; i < ExpTable.Length; i++){
            ExpTable[i] = (float)(BaseExpThreshold * Math.Pow(i, Exponent));
            if(i < 10) Console.WriteLine(ExpTable[i]);
        }
    }
}