#include "math.h"

char const *GAME_OBJECT = "PluginBridge";

@interface Utility : NSObject
@end

@implementation Utility

+ (NSString *)dictionaryToJson:(NSDictionary *)dictionary {
    NSError* error;
    NSData* jsonData = [NSJSONSerialization dataWithJSONObject:dictionary options:0 error:&error];
    if (!jsonData) {
        NSLog(@"Dictionary stringify error: %@", error);
        return @"";
    }
    return [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
}

@end

@interface NativeCalculationsPlugin : NSObject
@end

@implementation NativeCalculationsPlugin

static NativeCalculationsPlugin *_sharedInstance;

+(NativeCalculationsPlugin*)sharedInstance
{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _sharedInstance = [[NativeCalculationsPlugin alloc] init];
    });
    return _sharedInstance;
}

-(id)init
{
    self = [super init];
    if (self)
        [self initHelper];
    return self;
}

-(void)initHelper
{
    // Set up code goes here
    NSLog(@"Initialized NativeCalculationsPlugin class");
}

-(NSString *)performCalculations:(int)rectangleHeight rectangleWidth:(int) rectangleWidth
{
    double diagonal = sqrt(pow(rectangleHeight, 2) + pow(rectangleWidth, 2));
    int perimeter = 2 * rectangleHeight + 2 * rectangleWidth;
    int area = rectangleHeight * rectangleWidth;
    
    NSDictionary* calculationResults = @{
        @"diagonal": [NSNumber numberWithDouble:diagonal],
        @"perimeter": [NSNumber numberWithInt:perimeter],
        @"area": [NSNumber numberWithInt:area],
    };
    
    return [Utility dictionaryToJson:calculationResults];
}

@end

extern "C"
{
    char* cStringCopy(const char* string)
    {
        if (string == NULL)
            return NULL;

        char* res = (char*)malloc(strlen(string) + 1);
        strcpy(res, string);

        return res;
    }

    const char* syncCalculation(int rectangleHeight, int rectangleWidth)
    {
        try {
            NSLog(@"%@", [NSString stringWithFormat:@"syncCalculation for rectangleHeight: %d and rectangleWidth: %d", rectangleHeight, rectangleWidth]);
            NSString *calculationResults = [[NativeCalculationsPlugin sharedInstance] performCalculations:rectangleHeight rectangleWidth:rectangleWidth];
            return cStringCopy([calculationResults UTF8String]);
        }
        
        catch (NSException *exception) {
            UnitySendMessage(GAME_OBJECT, [@"HandleException" UTF8String], [[[exception callStackSymbols] componentsJoinedByString: @"\n"] UTF8String]);
            return nil;
        }
    }

    void asyncCalculation(int rectangleHeight, int rectangleWidth)
    {
        try {
            NSLog(@"%@", [NSString stringWithFormat:@"asyncCalculation for rectangleHeight: %d and rectangleWidth: %d", rectangleHeight, rectangleWidth]);
            // Assuming these calculations results required async methods
            NSString *calculationResults = [[NativeCalculationsPlugin sharedInstance] performCalculations:rectangleHeight rectangleWidth:rectangleWidth];
            UnitySendMessage(GAME_OBJECT, [@"HandleAsyncCalculation" UTF8String], [calculationResults UTF8String]);
        }
        
        catch (NSException *exception) {
            UnitySendMessage(GAME_OBJECT, [@"HandleException" UTF8String], [[[exception callStackSymbols] componentsJoinedByString: @"\n"] UTF8String]);
        }
    }
}
