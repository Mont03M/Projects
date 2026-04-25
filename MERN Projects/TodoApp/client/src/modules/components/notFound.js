import { CustomAppContainer } from "./customAppContainer";
import { CustomContentHeader } from "./customContentHeader";
import { CustomContentBody } from "./customContentBody";

export function NotFound() {
  return (
    <>
      <CustomAppContainer>
        <CustomContentHeader>
          <h1 className="text-center w-100">Page Not Found</h1>
        </CustomContentHeader>
        <CustomContentBody></CustomContentBody>
      </CustomAppContainer>
    </>
  );
}
