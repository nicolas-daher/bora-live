import { paths } from '../shared/paths.ts'
import { ButtonLink } from '../shared/ui/Button.tsx'
import { Page } from '../shared/ui/Page.tsx'

export function NotFoundPage() {
  return (
    <Page title="Opa, essa página não existe" lead="O endereço pode ter mudado ou sido digitado errado.">
      <div>
        <ButtonLink to={paths.home}>Voltar pro início</ButtonLink>
      </div>
    </Page>
  )
}
